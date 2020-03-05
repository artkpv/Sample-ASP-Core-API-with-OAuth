using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using BA.WebAPI.Model;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Model
{
    /// <summary> 
    ///     Compiles a WebAPI text filter into a System.Linq.Expressions.Expression predicate for data filtering.
    ///     
    /// </summary>
    /// <remarks>
    ///     Uses Roslyn compiler. 
    ///     See https://www.strathweb.com/2018/01/easy-way-to-create-a-c-lambda-expression-from-a-string-with-roslyn/
    /// </remarks>
    public class QueryFilterToPredicateExpressionCompiler<T>
    {
        private string filter;

        private readonly ILogger logger;

        private string cleanFilter;

        private bool isValid = true;

        private List<string> words = new List<string>();

        private Expression<Func<T, bool>> expression = null;

        private string entityName = nameof(T).ToLowerInvariant();

        private string[] entityProperties = typeof(T).GetProperties().Select(pr => pr.Name).ToArray();

        private static Dictionary<string, string> OperatorsTable = new Dictionary<string, string> {
            { "lt", "<" },
            { "le", "<=" },
            { "gt", ">" },
            { "ge", ">=" },
            { "eq", "==" },
            { "ne", "!=" },
            { "and", "&&" },
            { "or", "||" }
        };

        public QueryFilterToPredicateExpressionCompiler(string filter, ILogger logger)
        {
            this.filter = filter;
            this.logger = logger;
        }

        public async Task Compile()
        {
            BreakIntoWords();                
            if (isValid)
                ConstructCleanFilter();

            if (isValid)
                await CompileInternal();
        }

        public bool IsValid() => isValid;

        public Expression<Func<T, bool>> GetPredicate() => expression;

        private void BreakIntoWords()
        {
            if (String.IsNullOrWhiteSpace(filter))
                return;

            int n = filter.Length;

            int i = 0;
            while (i < n)
            {
                i = MoveToNextWord(i);
                i = AddWordOrBracket(i);
            }
            return;

            int MoveToNextWord(int i) 
            {
                while (i < n && char.IsWhiteSpace(filter[i]))
                    i += 1;
                return i;
            }

            int AddWordOrBracket(int i) 
            {
                if (i >= n)
                    return i;

                if (IsBracket(i))
                {
                    words.Add(filter[i].ToString());
                    i += 1;
                }
                else 
                {
                    i = AddWord(i);
                }
                return i;
            }

            bool IsBracket(int i) => filter[i] == '(' || filter[i] == ')';

            int AddWord(int start)
            {
                int end = start + 1;
                while (!IsWordEnd(start, end))
                    end += 1;

                if (InQuotes(start) && end < n)
                {
                    words.Add(filter.Substring(start + 1, end - start - 1));
                    return end + 1;
                }
                else 
                {
                    words.Add(filter.Substring(start, end - start));
                    return end;
                }
            }

            bool IsWordEnd(int start, int current) 
            {
                if (current >= n)
                    return true;
                if (InQuotes(start))
                    return filter[start] == filter[current];

                return IsBracket(current) || char.IsWhiteSpace(filter[current]);
            }
                
            bool InQuotes(int i)
                 => filter[i] == '\'' || filter[i] == '\"';
        }

        private void ConstructCleanFilter()
        {
            if (!words.Any())
            {
                isValid = false;
                return;
            }
            var sb = new StringBuilder();
            sb.Append(entityName);
            sb.Append(" => ");

            foreach (string w in words)
            {
                sb.Append(Translate(w));
                sb.Append(' ');
            }
            
            cleanFilter = sb.ToString();
            return;

            string Translate(string w)
            {
                if (IsOperator(w))
                    return TranslateOperator(w);
                else if (IsBracket(w))
                    return w;
                else if (IsProperty(w))
                    return TranslateProperty(w);
                else
                    return TranslateValue(w);
            }

            bool IsBracket(string w) 
                => w == "(" || w == ")";

            string TranslateValue(string w)
            {
                if (DateTime.TryParse(w, out DateTime result))
                    return $"System.DateTime.FromFileTime({result.ToUniversalTime().ToFileTime()})";
                return w;
            }

            string TranslateProperty(string w)
                => $"{entityName}.{PropertyToPathDictionary[w.ToLowerInvariant()]}";

            bool IsProperty(string w) 
                => PropertyToPathDictionary.ContainsKey(w.ToLowerInvariant());

            bool IsOperator(string w)
                => OperatorsTable.ContainsKey(w);

            string TranslateOperator(string w)
                => OperatorsTable[w];
        }

        private static Dictionary<string, string> _PropertyToPathDictionary = null;

        /// <summary>
        ///   Collects all possible properties for the entity.
        /// </summary>
        private static Dictionary<string, string> PropertyToPathDictionary
        {
            get
            {
                if (_PropertyToPathDictionary != null)
                    return _PropertyToPathDictionary;
                _PropertyToPathDictionary = new Dictionary<string, string>();
                var path = new List<string>();
                var visited = new HashSet<Type>();
                Visit(typeof(T));

                void Visit(Type t)
                {
                    if (visited.Contains(t))
                        return;
                    visited.Add(t);
                    foreach (PropertyInfo p in t.GetProperties())
                    {
                        path.Add(p.Name);
                        if (!_PropertyToPathDictionary.ContainsKey(p.Name.ToLowerInvariant()))
                            _PropertyToPathDictionary.Add(p.Name.ToLowerInvariant(), string.Join(".", path));
                        Visit(p.PropertyType);
                        path.RemoveAt(path.Count() - 1);
                    }
                }
                return _PropertyToPathDictionary;
            }
        }

        private async Task CompileInternal()
        {
            var options = ScriptOptions.Default.AddReferences(typeof(T).Assembly);
            options.AddImports("System");
            try 
            {
                expression = await CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(cleanFilter, options);
            }
            catch (CompilationErrorException cee)
            {
                logger.LogError($"Failed for '{cleanFilter}': {cee.Message}");
                isValid = false;
            }
        }
    }
}

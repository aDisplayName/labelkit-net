// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// <see cref="ILabelSelectorExpressionBuilder{TLabels}"/> for EFCore-MySql building expressions targeting labels
/// stored as JSON arrays of <c>name:value</c> strings.
/// </summary>
/// <param name="delimiter">Delimiter used to separate label names and values.</param>
/// <typeparam name="T">Type of labels.</typeparam>
public class MySqlCollectionLabelSelectorExpressionBuilder<T>(string delimiter = ":") : ILabelSelectorExpressionBuilder<T>
  where T : IEnumerable<string>
{
  public Expression<Func<T, bool>> Build(ILabelSelector selector)
  {
    var expression = PredicateBuilder.New<T>(true);

    var exactMatches = selector.GetExactMatches().Select(s => $"{s.name}{delimiter}{s.value}").ToArray();

    foreach (var label in exactMatches)
    {
      expression = expression.And(e => EF.Functions.JsonContains(e!, EF.Functions.JsonQuote(label)));
    }

    foreach (var selectorExpression in selector)
    {
      switch (selectorExpression)
      {
        case { Operator: LabelSelectorOperator.In, Values.Length: 1 }:
          continue;
        case { Operator: LabelSelectorOperator.In, Values.Length: > 0 }:
        {
          var orExpression = PredicateBuilder.New<T>(false);
          foreach (var value in selectorExpression.Values)
          {
            var label = $"{selectorExpression.Name}{delimiter}{value}";
            orExpression = orExpression.Or(e => EF.Functions.JsonContains(e!, EF.Functions.JsonQuote(label)));
          }

          expression = expression.And(orExpression);
          break;
        }
        case { Operator: LabelSelectorOperator.NotIn, Values.Length: > 0 }:
        {
          foreach (var value in selectorExpression.Values)
          {
            var label = $"{selectorExpression.Name}{delimiter}{value}";
            expression = expression.And(e => !EF.Functions.JsonContains(e!, EF.Functions.JsonQuote(label)));
          }

          break;
        }
        case { Operator: LabelSelectorOperator.Like, Values.Length: > 0 }:
        {
          var orExpression = PredicateBuilder.New<T>(false);
          foreach (var pattern in selectorExpression.Values)
          {
            orExpression = orExpression.Or(e =>
              Regex.IsMatch(
                EF.Functions.JsonUnquote(EF.Functions.JsonExtract<string>(e!, "$"))!,
                $"\"{selectorExpression.Name}{delimiter}{pattern}(?=\")"));
          }

          expression = expression.And(orExpression);
          break;
        }
        case { Operator: LabelSelectorOperator.NotLike, Values.Length: > 0 }:
        {
          foreach (var pattern in selectorExpression.Values)
          {
            expression = expression.And(e =>
              !Regex.IsMatch(
                EF.Functions.JsonUnquote(EF.Functions.JsonExtract<string>(e!, "$"))!,
                $"\"{selectorExpression.Name}{delimiter}{pattern}(?=\")"));
          }

          break;
        }
        case { Operator: LabelSelectorOperator.Exists }:
          expression = expression.And(e =>
            Regex.IsMatch(
              EF.Functions.JsonUnquote(EF.Functions.JsonExtract<string>(e!, "$"))!,
              $"\"{selectorExpression.Name}{delimiter}"));
          break;
        case { Operator: LabelSelectorOperator.NotExists }:
          expression = expression.And(e =>
            !Regex.IsMatch(
              EF.Functions.JsonUnquote(EF.Functions.JsonExtract<string>(e!, "$"))!,
              $"\"{selectorExpression.Name}{delimiter}"));
          break;
      }
    }

    return expression;
  }
}

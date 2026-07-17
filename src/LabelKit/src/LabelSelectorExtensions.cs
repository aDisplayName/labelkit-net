// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using Internal;

public static class LabelSelectorExtensions
{
  /// <summary>
  /// Match the specified label with an expression.
  /// </summary>
  /// <param name="selector">The selector to extend.</param>
  /// <param name="name">Name of the target label.</param>
  /// <returns></returns>
  public static LabelSelectorExpressionBuilder Match(this LabelSelector selector, string name)
    => new(selector, name);

  /// <summary>
  /// Extracts all expressions that represent an exact label match.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <returns></returns>
  public static IEnumerable<(string name, string value)> GetExactMatches(this ILabelSelector selector)
    => selector
      .Where(e => e.IsExactMatch())
      .Select(e => (e.Name, e.Values![0]));

  /// <summary>
  /// Combines two <see cref="ILabelSelector"/> instances (without materializing).
  /// </summary>
  /// <param name="selector">First selector.</param>
  /// <param name="other">Second selector.</param>
  /// <returns></returns>
  public static ILabelSelector Combine(this ILabelSelector selector, ILabelSelector other)
    => LabelSelector.Combine(selector, other);

  /// <summary>
  /// Merges two <see cref="ILabelSelector"/> instances, materializing the expressions and removing duplicates.
  /// </summary>
  /// <param name="selector">First selector.</param>
  /// <param name="other">Second selector.</param>
  /// <returns></returns>
  public static LabelSelector Merge(this ILabelSelector selector, ILabelSelector other)
    => LabelSelector.Merge(selector, other);

  /// <summary>
  /// Determines whether a given set of labels (represented by a collection of keys and values) matches the given <see cref="ILabelSelector"/>.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <param name="labels">Set of labels.</param>
  /// <param name="options">Matching options. When null, uses <see cref="LabelSelector.MatchingOptions"/> if available, otherwise <see cref="MatchingOptions.Default"/>.</param>
  /// <remarks></remarks>
  /// <returns></returns>
  public static bool Matches(this ILabelSelector selector, IEnumerable<KeyValuePair<string, string>> labels, MatchingOptions? options = null)
  {
    var resolvedOptions = ResolveMatchingOptions(selector, options);
    return selector.All(e => e.Matches(labels, resolvedOptions));
  }

  /// <summary>
  /// Determines whether a given set of labels (represented by a collection of strings) matches the given <see cref="ILabelSelector"/>.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <param name="labels">Set of labels.</param>
  /// <param name="delimiter">Delimiter used to separate label names and values.</param>
  /// <param name="options">Matching options. When null, uses <see cref="LabelSelector.MatchingOptions"/> if available, otherwise <see cref="MatchingOptions.Default"/>.</param>
  /// <returns></returns>
  public static bool Matches(this ILabelSelector selector, IEnumerable<string> labels, string delimiter = ":", MatchingOptions? options = null)
  {
    var resolvedOptions = ResolveMatchingOptions(selector, options);
    return selector.All(e => e.Matches(labels, delimiter, resolvedOptions));
  }

  /// <summary>
  /// Determines whether a given set of labels (represented by a collection of keys and values) matches the given <see cref="LabelSelectorExpression"/>.
  /// </summary>
  /// <param name="expression">The expression.</param>
  /// <param name="labels">Set of labels.</param>
  /// <param name="options">Matching options for Like / NotLike regular-expression evaluation.</param>
  /// <remarks></remarks>
  /// <returns></returns>
  public static bool Matches(this LabelSelectorExpression expression, IEnumerable<KeyValuePair<string, string>> labels, MatchingOptions options)
  {
    return expression switch
    {
      { Operator: LabelSelectorOperator.In, Values.Length: > 0 }
        => expression.Values.Any(v => labels.Contains(new KeyValuePair<string, string>(expression.Name, v))),
      { Operator: LabelSelectorOperator.NotIn, Values.Length: > 0 }
        => expression.Values.All(v => !labels.Contains(new KeyValuePair<string, string>(expression.Name, v))),
      { Operator: LabelSelectorOperator.Like, Values.Length: > 0 }
        => labels is IDictionary<string, string> dict
          ? dict.TryGetValue(expression.Name, out var likeValue) && expression.Values.Any(p => RegexMatchHelper.IsMatch(likeValue, p, options))
          : labels.Where(l => l.Key == expression.Name).Select(l => l.Value).Any(value => expression.Values.Any(p => RegexMatchHelper.IsMatch(value, p, options))),
      { Operator: LabelSelectorOperator.NotLike, Values.Length: > 0 }
        => labels is IDictionary<string, string> notLikeDict
          ? !notLikeDict.TryGetValue(expression.Name, out var notLikeValue) || expression.Values.All(p => !RegexMatchHelper.IsMatch(notLikeValue, p, options))
          : !labels.Any(l => l.Key == expression.Name) || labels.Where(l => l.Key == expression.Name).All(l => expression.Values.All(p => !RegexMatchHelper.IsMatch(l.Value, p, options))),
      { Operator: LabelSelectorOperator.Exists }
        => labels is IDictionary<string, string> dict ? dict.ContainsKey(expression.Name) : labels.Any(l => l.Key == expression.Name),
      { Operator: LabelSelectorOperator.NotExists }
        => labels is IDictionary<string, string> dict ? !dict.ContainsKey(expression.Name) : labels.All(l => l.Key != expression.Name),
      _ => true
    };
  }

  /// <summary>
  /// Determines whether a given set of labels (represented by a collection of strings) matches the given <see cref="LabelSelectorExpression"/>.
  /// </summary>
  /// <param name="expression">The expression.</param>
  /// <param name="labels">Set of labels.</param>
  /// <param name="delimiter">Delimiter used to separate label names and values.</param>
  /// <param name="options">Matching options for Like / NotLike regular-expression evaluation.</param>
  /// <returns></returns>
  public static bool Matches(this LabelSelectorExpression expression, IEnumerable<string> labels, string delimiter, MatchingOptions options)
  {
    return expression switch
    {
      { Operator: LabelSelectorOperator.In, Values.Length: > 0 }
        => expression.Values.Any(v => labels.Contains($"{expression.Name}{delimiter}{v}")),
      { Operator: LabelSelectorOperator.NotIn, Values.Length: > 0 }
        => expression.Values.All(v => !labels.Contains($"{expression.Name}{delimiter}{v}")),
      { Operator: LabelSelectorOperator.Like, Values.Length: > 0 }
        => labels
          .Where(l => l.StartsWith($"{expression.Name}{delimiter}"))
          .Select(l => l[(expression.Name.Length + delimiter.Length)..])
          .Any(value => expression.Values.Any(p => RegexMatchHelper.IsMatch(value, p, options))),
      { Operator: LabelSelectorOperator.NotLike, Values.Length: > 0 }
        => !labels.Any(l =>
          l.StartsWith($"{expression.Name}{delimiter}") &&
          expression.Values.Any(p => RegexMatchHelper.IsMatch(l[(expression.Name.Length + delimiter.Length)..], p, options))),
      { Operator: LabelSelectorOperator.Exists }
        => labels.Any(l => l.StartsWith(expression.Name)),
      { Operator: LabelSelectorOperator.NotExists }
        => labels.All(l => !l.StartsWith(expression.Name)),
      _ => true
    };
  }

  /// <summary>
  /// Retrieves all <see cref="LabelSelectorExpression"/>s targeting the specified label.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <param name="name">Name of the label.</param>
  /// <returns></returns>
  public static IEnumerable<LabelSelectorExpression> ForLabel(this ILabelSelector selector, string name)
    => selector.Where(e => e.Name == name);

  /// <summary>
  /// Retrieves a list of labels targeted by the specified <see cref="ILabelSelector"/>.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <returns></returns>
  public static IEnumerable<string> Labels(this ILabelSelector selector)
    => selector.Select(e => e.Name).Distinct();

  private static MatchingOptions ResolveMatchingOptions(ILabelSelector selector, MatchingOptions? options)
    => options ?? (selector as LabelSelector)?.MatchingOptions ?? MatchingOptions.Default;
}

// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

/// <summary>
/// Instance allowing a fluent extension of <see cref="LabelSelector"/>s.
/// </summary>
/// <param name="selector">The selector to extend.</param>
/// <param name="name">Name of the target label.</param>
public class LabelSelectorExpressionBuilder(LabelSelector selector, string name)
{
  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.In"/>.
  /// </summary>
  /// <param name="values">Values to match.</param>
  /// <returns></returns>
  public LabelSelector In(IEnumerable<string> values)
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.In,
      Values = values.ToArray()
    });

    return selector;
  }

  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.NotIn"/>.
  /// </summary>
  /// <param name="values">Values to match.</param>
  /// <returns></returns>
  public LabelSelector NotIn(IEnumerable<string> values)
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.NotIn,
      Values = values.ToArray()
    });

    return selector;
  }

  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.Like"/>.
  /// </summary>
  /// <param name="patterns">Regular expression patterns to match.</param>
  /// <returns></returns>
  public LabelSelector Like(IEnumerable<string> patterns)
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.Like,
      Values = patterns.ToArray()
    });

    return selector;
  }

  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.NotLike"/>.
  /// </summary>
  /// <param name="patterns">Regular expression patterns to match.</param>
  /// <returns></returns>
  public LabelSelector NotLike(IEnumerable<string> patterns)
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.NotLike,
      Values = patterns.ToArray()
    });

    return selector;
  }

  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.Exists"/>.
  /// </summary>
  /// <returns></returns>
  public LabelSelector Exists()
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.Exists
    });

    return selector;
  }

  /// <summary>
  /// Adds a <see cref="LabelSelectorExpression"/> with the operator <see cref="LabelSelectorOperator.NotExists"/>.
  /// </summary>
  /// <returns></returns>
  public LabelSelector NotExists()
  {
    selector.Expressions ??= new List<LabelSelectorExpression>();

    selector.Expressions.Add(new LabelSelectorExpression
    {
      Name = name,
      Operator = LabelSelectorOperator.NotExists
    });

    return selector;
  }
}

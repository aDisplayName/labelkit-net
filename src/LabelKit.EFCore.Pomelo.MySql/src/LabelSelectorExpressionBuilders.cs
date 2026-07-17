// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

public static partial class MySqlLabelSelectorExpressionBuilders
{
  internal static class Builders<TLabels>
    where TLabels : IDictionary<string, string>
  {
    public static ILabelSelectorExpressionBuilder<TLabels> Json
      => new MySqlJsonLabelSelectorExpressionBuilder<TLabels>();
  }

  /// <inheritdoc cref="MySqlJsonLabelSelectorExpressionBuilder{T}"/>
  public static ILabelSelectorExpressionBuilder<TLabels> Json<TLabels>()
    where TLabels : IDictionary<string, string>
    => Builders<TLabels>.Json;

  /// <summary>
  /// Creates an expression builder for labels stored as JSON arrays of <c>name:value</c> strings.
  /// </summary>
  /// <typeparam name="TLabels">Type of labels.</typeparam>
  /// <returns>The expression builder.</returns>
  public static ILabelSelectorExpressionBuilder<TLabels> Collection<TLabels>()
    where TLabels : IEnumerable<string>
    => new MySqlCollectionLabelSelectorExpressionBuilder<TLabels>();
}

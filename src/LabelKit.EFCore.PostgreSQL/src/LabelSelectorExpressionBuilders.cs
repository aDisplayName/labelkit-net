// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

public static partial class NpgsqlLabelSelectorExpressionBuilders
{
  internal static class Builders<TLabels>
    where TLabels : IDictionary<string, string>
  {
    public static ILabelSelectorExpressionBuilder<TLabels> Jsonb
      => new NpgsqlJsonbLabelSelectorExpressionBuilder<TLabels>();
  }

  /// <inheritdoc cref="NpgsqlJsonbLabelSelectorExpressionBuilder{T}"/>
  public static ILabelSelectorExpressionBuilder<TLabels> Jsonb<TLabels>()
    where TLabels : IDictionary<string, string>
    => Builders<TLabels>.Jsonb;
}

// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
/// EF Core model configuration extensions for LabelKit MySQL support.
/// </summary>
public static class MySqlLabelKitModelBuilderExtensions
{
  /// <summary>
  /// Registers MySQL functions required for LabelKit JSON label selectors.
  /// </summary>
  /// <param name="modelBuilder">The model builder.</param>
  /// <returns>The model builder.</returns>
  public static ModelBuilder ConfigureLabelKitMySql(this ModelBuilder modelBuilder)
  {
    var methodInfo = typeof(MySqlLabelKitDbFunctions).GetMethod(
      nameof(MySqlLabelKitDbFunctions.JsonGetText),
      [typeof(object), typeof(string)])!;

    var functionBuilder = modelBuilder.HasDbFunction(methodInfo);

    functionBuilder.HasParameter("json").HasStoreType("json");

    functionBuilder.HasTranslation(args =>
    {
      var stringTypeMapping = args[1]!.TypeMapping;

      var extracted = new SqlFunctionExpression(
        "JSON_EXTRACT",
        [args[0]!, args[1]!],
        nullable: true,
        argumentsPropagateNullability: [false, false],
        typeof(string),
        stringTypeMapping);

      return new SqlFunctionExpression(
        "JSON_UNQUOTE",
        [extracted],
        nullable: true,
        argumentsPropagateNullability: [true],
        typeof(string),
        stringTypeMapping);
    });

    return modelBuilder;
  }
}

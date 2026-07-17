// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
/// EF Core model configuration extensions for LabelKit PostgreSQL support.
/// </summary>
public static class NpgsqlLabelKitModelBuilderExtensions
{
  /// <summary>
  /// Registers PostgreSQL functions required for LabelKit JSONB label selectors.
  /// </summary>
  /// <param name="modelBuilder">The model builder.</param>
  /// <returns>The model builder.</returns>
  public static ModelBuilder ConfigureLabelKitNpgsql(this ModelBuilder modelBuilder)
  {
    var methodInfo = typeof(NpgsqlLabelKitDbFunctions).GetMethod(
      nameof(NpgsqlLabelKitDbFunctions.JsonbGetText),
      [typeof(object), typeof(string)])!;

    var functionBuilder = modelBuilder.HasDbFunction(methodInfo);

    functionBuilder.HasParameter("json").HasStoreType("jsonb");

    functionBuilder.HasTranslation(args =>
        new SqlFunctionExpression(
          "jsonb_extract_path_text",
          args,
          nullable: true,
          argumentsPropagateNullability: [false, false],
          typeof(string),
          typeMapping: null));

    return modelBuilder;
  }
}

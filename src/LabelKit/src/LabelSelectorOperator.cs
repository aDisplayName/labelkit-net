// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

public enum LabelSelectorOperator
{
  /// <summary>
  /// Match label against a set of values where any of the values should match.
  /// </summary>
  In,
  /// <summary>
  /// Match label against a set of values where all the values should not match.
  /// </summary>
  NotIn,
  /// <summary>
  /// Match label value against a set of regular expressions where any of the patterns should match.
  /// </summary>
  Like,
  /// <summary>
  /// Match label value against a set of regular expressions where none of the patterns should match.
  /// </summary>
  NotLike,
  /// <summary>
  /// Match label for existence without considering the value.
  /// </summary>
  Exists,
  /// <summary>
  /// Match label for non-existence without considering the value.
  /// </summary>
  NotExists
}

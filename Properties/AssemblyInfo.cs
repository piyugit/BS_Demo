using NUnit.Framework;

// Enable test parallel execution at fixture (Feature) level
[assembly: Parallelizable(ParallelScope.Children)]

// Allow up to 5 concurrent threads/scenarios
[assembly: LevelOfParallelism(5)]
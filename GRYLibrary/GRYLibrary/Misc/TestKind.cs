namespace GRYLibrary.Core.Misc
{
    public enum TestKind
    {
        /// <summary>
        /// Tests that a component work like defined.
        /// </summary>
        UnitTest = 0,

        /// <summary>
        /// Tests that 2 (or more) components work together like defined.
        /// </summary>
        IntegrationTest = 1,

        /// <summary>
        /// Tests that are not doing anything when committing but can be useful to do or debug a certain task/function.
        /// </summary>
        DebugHelperTest = 2,

        /// <summary>
        /// Tests that other required systems/services used by the function under test are still available and work like specified so that the function under test is able to be working correctly.
        /// </summary>
        SystemTest = 3,

        /// <summary>
        /// Tests that will be done by reflection to assert a certain property for all types or all methods or all attributes in a certain scope.
        /// </summary>
        ReflectionTest = 4,

        /// <summary>
        /// Verifies that recent changes did not break anything.
        /// </summary>
        RegressionTest = 5,

        /// <summary>
        /// Verifies that recent changes did not change the UI.
        /// </summary>
        VisualRegressionTest = 6,

        /// <summary>
        /// Tests that verifies a certain performance for a certain operation. Testcases of this kind often require appropriate hardware.
        /// </summary>
        PerformanceTest = 7,

        /// <summary>
        /// Tests that verifies that the entire communication between 2 systems is working correctly including serialization, transport and deserialization.
        /// </summary>
        EndToEndTest = 8,

        /// <summary>
        /// Test that only generates build-artifacts.
        /// </summary>
        GenerationTest = 9,

        /// <summary>
        /// Verifies that a certain function is working correctly in a minimal environment with minimal dependencies.
        /// </summary>
        SmokeTest = 10,
    }
}
namespace DataClasses
{
    // used in JSON so only add new tasks at the end
    public enum AgentTask
    {
        ContextChangeIdentification,
        ContextUpdating,
        MultiDefinitionResolution,
        SingleDefinitionResolution,
        DecompositionToStatements,
        DecompositionToUnits,
        UnitRooting,
        DefinitionParsing,
        DefinitionIPAPronouncing,
        DefinitionRomanisedPronouncing,
        DefinitionRewriting,
        DefinitionExplaining,
        GeneralPurpose,
    }

}

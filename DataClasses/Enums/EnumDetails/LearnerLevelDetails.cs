namespace DataClasses
{
    public static class LearnerLevelDetails
    {
        public static List<LearnerLevel> LearnerLevels()
        {
            List<LearnerLevel> levels = new List<LearnerLevel>();
            foreach (LearnerLevel lev in Enum.GetValues(typeof(LearnerLevel)))
            {
                levels.Add(lev);
            }
            return levels;
        }

        // TODO - make this translate to other languages
        public static List<String> LearnerLevelNames()
        {
            return new List<String> { "Beginner", "Elementary", "Intermediate", "Advanced", "Native" };
        }
    }
}

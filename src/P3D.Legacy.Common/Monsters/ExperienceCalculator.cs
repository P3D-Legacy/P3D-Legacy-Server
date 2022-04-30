using System;
using System.Runtime.Serialization;

namespace P3D.Legacy.Common.Monsters
{
    [Serializable]
    public class ExperienceCalculatorException : Exception
    {
        protected ExperienceCalculatorException() { }
        protected ExperienceCalculatorException(string message) : base(message) { }
        protected ExperienceCalculatorException(string message, Exception innerException) : base(message, innerException) { }
        protected ExperienceCalculatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class IncorrectExperienceTypeExperienceCalculatorException : ExperienceCalculatorException
    {
        public IncorrectExperienceTypeExperienceCalculatorException() { }
        public IncorrectExperienceTypeExperienceCalculatorException(string message) : base(message) { }
        public IncorrectExperienceTypeExperienceCalculatorException(string message, Exception innerException) : base(message, innerException) { }
        protected IncorrectExperienceTypeExperienceCalculatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class LevelTooHighExperienceCalculatorException : ExperienceCalculatorException
    {
        public LevelTooHighExperienceCalculatorException() { }
        public LevelTooHighExperienceCalculatorException(string message) : base(message) { }
        public LevelTooHighExperienceCalculatorException(string message, Exception innerException) : base(message, innerException) { }
        protected LevelTooHighExperienceCalculatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public static class ExperienceCalculator
    {
        public static byte LevelForExperienceValue(ExperienceType experienceType, long experience)
        {
            // returns level 1 if no experience (or negative value):
            if (experience <= 0)
                return 1;

            byte level = 1;
            while (ExperienceNeededForLevel(experienceType, level) < experience)
                level++;
            level--;

            return level;
        }

        public static int ExperienceNeededForLevel(ExperienceType experienceType, int level) => experienceType switch
        {
            ExperienceType.Erratic => (int) Math.Round(ExperienceNeededForLevelErratic(level)),
            ExperienceType.Fast => (int) Math.Round(ExperienceNeededForLevelFast(level)),
            ExperienceType.MediumFast => (int) Math.Round(ExperienceNeededForLevelMediumFast(level)),
            ExperienceType.MediumSlow => (int) Math.Round(ExperienceNeededForLevelMediumSlow(level)),
            ExperienceType.Slow => (int) Math.Round(ExperienceNeededForLevelSlow(level)),
            ExperienceType.Fluctuating => (int) Math.Round(ExperienceNeededForLevelFluctuating(level)),
            _ => throw new IncorrectExperienceTypeExperienceCalculatorException($"ExperienceType {(int) experienceType} is incorrect!")
        };

        private static double ExperienceNeededForLevelErratic(int level)
        {
            // EXP =
            // level <= 50:         ((pow(level, 3) * (100 - level)) / 50)
            // 50 <= level <= 68:   ((pow(level, 3) * (150 - level)) / 100)
            // 68 <= level <= 98:   ((pow(level, 3) * floor((1911 - (10 * level)) / 3)) / 500)
            // 98 <= level <= 100:  ((pow(level, 3) * (160 - level)) / 100)

            return level switch
            {
                <= 50 => (Math.Pow(level, 3D) * (100D - level)) / 50D,
                >= 50 and <= 68 => (Math.Pow(level, 3D) * (150D - level)) / 100D,
                >= 68 and <= 98 => (Math.Pow(level, 3D) * Math.Floor((1911D - (10D * level)) / 3D)) / 500D,
                >= 98 and <= 100 => (Math.Pow(level, 3D) * (160D - level)) / 100D,
                _ => throw new LevelTooHighExperienceCalculatorException($"Level {level} is too high for Erratic!")
            };
        }

        private static double ExperienceNeededForLevelFast(int level)
        {
            // EXP =
            // ((4 * pow(level, 3)) / 5)

            return (4D * Math.Pow(level, 3D)) / 5D;
        }

        private static double ExperienceNeededForLevelMediumFast(int level)
        {
            // EXP =
            // (pow(level, 3))

            return Math.Pow(level, 3D);
        }

        private static double ExperienceNeededForLevelMediumSlow(int level)
        {
            // EXP =
            // (((6 / 5) * pow(level, 3)) - (15 * pow(level, 2)) + (100 * level) - 140)

            return ((6D / 5D) * Math.Pow(level, 3D)) - (15D * Math.Pow(level, 2D)) + (100D * level) - 140D;
        }

        private static double ExperienceNeededForLevelSlow(int level)
        {
            // EXP =
            // ((5 * pow(level, 3)) / 4)

            return (5D * Math.Pow(level, 3D)) / 4D;
        }

        private static double ExperienceNeededForLevelFluctuating(int level)
        {
            // EXP =
            // level <= 15: (pow(level, 3) * ((floor((level + 1) / 3) + 24) / 50))
            // 15 <= level <= 36: (pow(level, 3) * ((level + 14) / 50))
            // 36 <= level <= 100: (pow(level, 3) * ((floor(level / 2) + 32) / 50))

            return level switch
            {
                <= 15 => Math.Pow(level, 3D) * ((Math.Floor((level + 1D) / 3D) + 24D) / 50D),
                >= 15 and <= 36 => Math.Pow(level, 3D) * ((level + 14D) / 50D),
                >= 36 and <= 100 => Math.Pow(level, 3D) * ((Math.Floor(level / 2D) + 32D) / 50D),
                _ => throw new LevelTooHighExperienceCalculatorException($"Level {level} is too high for Fluctuating!")
            };
        }
    }
}
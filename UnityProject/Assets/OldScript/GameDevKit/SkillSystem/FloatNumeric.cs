namespace GameDevKit.SkillSystem
{
    public struct FloatNumeric
    {
        private float baseValue;
        private float additionalValue;
        private float multiplyPercent;

        public FloatNumeric(float baseValue, float additionalValue = 0f, float multiplyPercent = 0f)
        {
            this.baseValue = baseValue;
            this.additionalValue = additionalValue;
            this.multiplyPercent = multiplyPercent;
        }

        public float Value
        {
            get { return (baseValue + additionalValue) * (1f + multiplyPercent); }
            set { baseValue = value; }
        }

        public float BaseValue
        {
            get { return baseValue; }
            set { baseValue = value; }
        }

        public float AdditionalValue
        {
            get { return additionalValue; }
            set { additionalValue = value; }
        }

        public float MultiplyPercent
        {
            get { return multiplyPercent; }
            set { multiplyPercent = value; }
        }

        public static implicit operator FloatNumeric(float value)
        {
            return new FloatNumeric(value);
        }

        public static implicit operator float(FloatNumeric numeric)
        {
            return numeric.Value;
        }

        public static FloatNumeric operator +(FloatNumeric a, FloatNumeric b)
        {
            return new FloatNumeric(a.baseValue + b.baseValue, a.additionalValue + b.additionalValue, a.multiplyPercent + b.multiplyPercent);
        }

        public static FloatNumeric operator *(FloatNumeric a, FloatNumeric b)
        {
            return new FloatNumeric(
                a.baseValue * b.baseValue,
                a.additionalValue + b.additionalValue,
                a.multiplyPercent + b.multiplyPercent + a.multiplyPercent * b.multiplyPercent
            );
        }
    }
}
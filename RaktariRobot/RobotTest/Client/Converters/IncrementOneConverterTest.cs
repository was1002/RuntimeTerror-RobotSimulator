using Xunit;

using RuntimeTerror.Client;

namespace RobotTest.Client.Converters
{
    public class IncrementOneConverterTest
    {
        [Theory]
        [InlineData(5,6)]
        [InlineData(0,1)]
        [InlineData(-7,-6)]
        public void Convert_ShouldAddOneToInteger(int input, int expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.Convert(input, typeof(int), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, 2)]
        [InlineData(false, 1)]
        public void Convert_ShouldAddOneToBool(bool input, int expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.Convert(input, typeof(int), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5.3, 6.3)]
        [InlineData(0.0, 1.0)]
        [InlineData(-7.12, -6.12)]
        public void Convert_ShouldAddOneToDouble(double input, double expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.Convert(input, typeof(double), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("123", "123")]
        [InlineData("a", "a")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void Convert_ShouldReturnSameString(string input, string expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.Convert(input, typeof(string), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(6, 5)]
        [InlineData(1, 0)]
        [InlineData(-6, -7)]
        public void ConvertBack_ShouldSubtractOneFromInteger(int input, int expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.ConvertBack(input, typeof(int), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, -1)]
        public void ConvertBack_ShouldSubtractOneFromBool(bool input, int expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.ConvertBack(input, typeof(int), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(6.3, 5.3)]
        [InlineData(1.0, 0.0)]
        [InlineData(-6.12, -7.12)]
        public void ConvertBack_ShouldSubtractOneFromDouble(double input, double expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.ConvertBack(input, typeof(double), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("123", "123")]
        [InlineData("a", "a")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void ConvertBack_ShouldReturnSameString(string input, string expected)
        {
            var converter = new IncrementOneConverter();
            var result = converter.ConvertBack(input, typeof(string), null, null);
            Assert.Equal(expected, result);
        }
    }
}

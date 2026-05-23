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
    }
}

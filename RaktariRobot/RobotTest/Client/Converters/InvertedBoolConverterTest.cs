using Xunit;

using RuntimeTerror.Client;

namespace RobotTest.Client.Converters
{
    public class InvertedBoolConverterTest
    {
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Convert_ShouldInvertBoolean(bool input, bool expected)
        {
            var converter = new InvertedBoolConverter();
            var result = converter.Convert(input, typeof(bool), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, true)]
        [InlineData("test", true)]
        public void Convert_ShouldReturnInputIfNotBoolean(object input, object expected)
        {
            var converter = new InvertedBoolConverter();
            var result = converter.Convert(input, typeof(bool), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ConvertBack_ShouldInvertBoolean(bool input, bool expected)
        {
            var converter = new InvertedBoolConverter();
            var result = converter.ConvertBack(input, typeof(bool), null, null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, true)]
        [InlineData("test", true)]
        public void ConvertBack_ShouldReturnInputIfNotBoolean(object input, object expected)
        {
            var converter = new InvertedBoolConverter();
            var result = converter.ConvertBack(input, typeof(bool), null, null);
            Assert.Equal(expected, result);
        }
    }
}

using EventlogAzureMonitorBridge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventlogAzureMonitorBridgeTest
{
    [TestClass]
    public class EventMessageConverterTest
    {
        [TestMethod]
        public void TestMethod_001()
        {
            var str = @"abc%%279de";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("abcUndefined Access (no effect) Bit 7de", ret);
        }
        [TestMethod]
        public void TestMethod_002()
        {
            var str = @"abc%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("abcUndefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_002b()
        {
            var str = @"%%279abc";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("Undefined Access (no effect) Bit 7abc", ret);
        }
        [TestMethod]
        public void TestMethod_002c()
        {
            var str = @"%%%279abc";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("%Undefined Access (no effect) Bit 7abc", ret);
        }
        [TestMethod]
        public void TestMethod_002d()
        {
            var str = @"abc%%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("abc%Undefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_003()
        {
            var str = @"abc%%000de";  // number not found
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(ret, ret);
        }
        [TestMethod]
        public void TestMethod_004()
        {
            var str = @"abc%%000";  // number not found
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(ret, ret);
        }
        [TestMethod]
        public void TestMethod_005()
        {
            var str = @"abc";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(ret, ret);
        }
        [TestMethod]
        public void TestMethod_006()
        {
            var str = @"%%279%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("Undefined Access (no effect) Bit 7Undefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_007()
        {
            var str = @"%%000%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("%%000Undefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_008()
        {
            var str = @"%%000%%279a";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("%%000Undefined Access (no effect) Bit 7a", ret);
        }
        [TestMethod]
        public void TestMethod_009()
        {
            var str = @"%%279%%000";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("Undefined Access (no effect) Bit 7%%000", ret);
        }
        [TestMethod]
        public void TestMethod_010()
        {
            var str = @"%%279%%000a";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("Undefined Access (no effect) Bit 7%%000a", ret);
        }
        [TestMethod]
        public void TestMethod_011()
        {
            var str = @"%%%%%%%%%%";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(str, ret);
        }
        [TestMethod]
        public void TestMethod_012()
        {
            var str = @"%%%%%%%%%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("%%%%%%%%Undefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_013()
        {
            var str = @"%%%%%%%%%%%279";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("%%%%%%%%%Undefined Access (no effect) Bit 7", ret);
        }
        [TestMethod]
        public void TestMethod_014()
        {
            var str = @"abc%%%279%";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual("abc%Undefined Access (no effect) Bit 7%", ret);
        }
        [TestMethod]
        public void TestMethod_015()
        {
            var str = @"abc%%%270%";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(str, ret);
        }
        [TestMethod]
        public void TestMethod_016()
        {
            var str = @"abc%%%270%%";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(str, ret);
        }
        [TestMethod]
        public void TestMethod_017()
        {
            var str = @"abc%%%270%%a";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(str, ret);
        }
        [TestMethod]
        public void TestMethod_018()
        {
            var str = @"abc%%%270%%2";
            var ret = EventMessageConverter.ConvertFrom(str);
            Assert.AreEqual(str, ret);
        }

    }
}

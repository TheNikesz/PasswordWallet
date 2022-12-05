using NUnit.Framework;
using NUnit.Framework.Constraints;
using PasswordWallet.Services;

namespace Services.Test;

public class HashingServiceTest
{
    private HashingService _hashService;

    static string[] HashLengthCases =
    {
        "",
        "P@ssw0rd",
    };

    [SetUp]
    public void Setup()
    {
        _hashService = new HashingService();
    }

    [Test]
    [TestCaseSource(nameof(HashLengthCases))]
    public void HashWithSHA512_ValidArgument_ReturnsHashWithCorrectLength(string text)
    {
        string actual = _hashService.HashWithSHA512(text);
        Assert.AreEqual(88, actual.Length);
    }

    [Test]
    [TestCase("", "6bI13GNvIh3K3qNNiHd9+3VVTPINgsMYTQdbhKZIINM7mqk/2Dfaiit+Rhlc6mSmuiS2VlxLZDZ8odSrn8UZDA==")]
    [TestCase("P@ssw0rd", "sws8s6SvuBXg2fkdSoWIcCjLcM7OdguB67JGpzn9E3dMGxrHLBwzmc3R8nZ9tHWuSdfrcjh3UbEC7CfgGoVKEw==")]
    public void HashWithSHA512_ValidArgument_ReturnsCorrectHash(string text, string expectedHash)
    {
        string actual = _hashService.HashWithSHA512(text);
        Assert.AreEqual(expectedHash, actual);
    }

    [Test]
    [TestCaseSource(nameof(HashLengthCases))]
    public void HashWithHMac_ValidArgument_ReturnsHashWithCorrectLength(string text)
    {
        string actual = _hashService.HashWithHMAC(text, "16characterssalt");
        Assert.AreEqual(88, actual.Length);
    }

    [Test]
    [TestCase("", "6uKCYacM20VGfEGeza7Jqz7WL/jT2mNOB/X9pibIPaCBcug5x70ZdYbHeCb2hiJVK7Of+SEbGbI7WT4RjGf4bA==")]
    [TestCase("P@ssw0rd", "OO9jtjOUhSJNk1ZzOHvk/lYGeMuuoIyb2c6BNcmI19vwtCSIDhOZ927NDh9k5PKIwuJDHQi99VANr5iMn9xfIQ==")]
    public void HashWithHMac_ValidArgument_ReturnsCorrectHash(string text, string expectedHash)
    {
        string actual = _hashService.HashWithHMAC(text, "16characterssalt");
        Assert.AreEqual(expectedHash, actual);
    }

    [Test]
    [TestCaseSource(nameof(HashLengthCases))]
    public void HashWithMD5_ValidArgument_ReturnsHashWithCorrectLength(string text)
    {
        string actual = _hashService.HashWithMD5(text);
        Assert.AreEqual(24, actual.Length);
    }

    [Test]
    [TestCase("", "1B2M2Y8AsgTpgAmY7PhCfg==")]
    [TestCase("P@ssw0rd", "Fh69fUUImzRG7k4NhtvPkg==")]
    public void HashWithMD5_ValidArgument_ReturnsCorrectHash(string text, string expectedHash)
    {
        string actual = _hashService.HashWithMD5(text);
        Assert.AreEqual(expectedHash, actual);
    }
}
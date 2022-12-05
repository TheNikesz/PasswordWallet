using NUnit.Framework;
using NUnit.Framework.Constraints;
using PasswordWallet.Services;
using System.Text;

namespace Services.Test;

public class CryptographicServiceTest
{
    private CryptographicService _cryptoService;

    static string[] EncryptCases =
    {
        "",
        "P@ssw0rd",
    };

    [SetUp]
    public void Setup()
    {
        _cryptoService = new CryptographicService();
    }

    [Test]
    [TestCaseSource(nameof(EncryptCases))]
    public void Encrypt_ValidArgument_ReturnsEcryptedPasswordhWithCorrectLength(string plainText)
    {
        byte[] masterPassword = new byte[] { 50, 222, 212, 195, 182, 37, 48, 40, 165, 39, 125, 219, 254, 216, 130, 3 };
        byte[] aesIv = new byte[] { 231, 216, 214, 32, 226, 66, 67, 117, 86, 145, 145, 244, 2, 143, 108, 0 };

        string actual = _cryptoService.Encrypt(plainText, masterPassword, aesIv);
        Assert.AreEqual(24, actual.Length);
    }

    [Test]
    [TestCase("", "4H/eSB6pjRTr68L0xYTPyQ==")]
    [TestCase("P@ssw0rd", "C9i6eopfIcZz3yTh+ojcsA==")]
    public void Encrypt_ValidArgument_ReturnsEcryptedPassword(string plainText, string expectedEncyptedPassword)
    {
        byte[] masterPassword = new byte[] { 50, 222, 212, 195, 182, 37, 48, 40, 165, 39, 125, 219, 254, 216, 130, 3 };
        byte[] aesIv = new byte[] { 231, 216, 214, 32, 226, 66, 67, 117, 86, 145, 145, 244, 2, 143, 108, 0 };

        string actual = _cryptoService.Encrypt(plainText, masterPassword, aesIv);
        Assert.AreEqual(expectedEncyptedPassword, actual);
    }

    [Test]
    [TestCase("4H/eSB6pjRTr68L0xYTPyQ==", "")]
    [TestCase("C9i6eopfIcZz3yTh+ojcsA==", "P@ssw0rd")]
    public void Decrypt_ValidArgument_ReturnsDecryptedPassword(string plainText, string expectedPassword)
    {

        byte[] masterPassword = new byte[] { 50, 222, 212, 195, 182, 37, 48, 40, 165, 39, 125, 219, 254, 216, 130, 3 };
        byte[] aesIv = new byte[] { 231, 216, 214, 32, 226, 66, 67, 117, 86, 145, 145, 244, 2, 143, 108, 0 };

        string actual = _cryptoService.Decrypt(plainText, masterPassword, aesIv);
        Assert.AreEqual(expectedPassword, actual);
    }
}
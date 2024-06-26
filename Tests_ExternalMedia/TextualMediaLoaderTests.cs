﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using UserInputInterfaces;
using ExternalMedia;
using Infrastructure;

namespace Tests_ExternalMedia
{
    [TestClass]
    public class TextualMediaLoaderTests
    {
        private Mock<ICanVerify> mockVerifierTrue;
        private Mock<ICanVerify> mockVerifierFalse;
        private Mock<ICanChooseFromList> mockChooser;
        private Mock<ICanGetText> mockTextGetter;
        private TextualMediaImporter loader;

        private string testFilePath = "test.txt";

        [TestInitialize]
        public void Setup()
        {
            // Mock ICanVerify that always returns true
            mockVerifierTrue = new Mock<ICanVerify>();
            mockVerifierTrue.Setup(m => m.AskYesNo(It.IsAny<string>())).Returns(true);

            mockChooser = new Mock<ICanChooseFromList>();

            mockTextGetter = new Mock<ICanGetText>();
            mockTextGetter.Setup(m => m.GetResponse(It.IsAny<string>())).Returns("a short description");

            // Create instances of TextualMediaLoader with different verifiers
            loader = new TextualMediaImporter(mockVerifierTrue.Object, mockChooser.Object, mockTextGetter.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadFromFile_InvalidPath_ThrowsArgumentException()
        {
            loader.ImportFromFile(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadFromFile_NonexistentFile_ThrowsFileNotFoundException()
        {
            string path = "nonexistent.txt";
            loader.ImportFromFile(path);
        }

        [TestMethod]
        public void LoadFromFile_ValidFile_ReturnsInternalTextualMediaWithCorrectText()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFile_ReturnsInternalTextualMediaWithCorrectDescription()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual("a short description", result.Description);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileASCII_ReturnsInternalTextualMedia()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent,Encoding.ASCII);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileUTF8_ReturnsInternalTextualMedia()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent, Encoding.UTF8);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void LoadFromFile_ValidFileUnicode_ReturnsInternalTextualMedia()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent, Encoding.Unicode);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileLatin1_ReturnsInternalTextualMedia()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent, Encoding.Latin1);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileBE_ReturnsInternalTextualMedia()
        {
            string path = "test.txt";
            string testContent = "Hello World";
            File.WriteAllText(path, testContent, Encoding.BigEndianUnicode);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileChinese_ReturnsInternalTextualMedia()
        {
            string path = "test_chinese.txt";
            string testContent = "你好世界"; // "Hello World" in Chinese
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileArabic_ReturnsInternalTextualMedia()
        {
            string path = "test_arabic.txt";
            string testContent = "مرحبا بالعالم"; // "Hello World" in Arabic
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileEmoji_ReturnsInternalTextualMedia()
        {
            string path = "test_emoji.txt";
            string testContent = "🌍👋"; // Earth and waving hand emojis
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }

        [TestMethod]
        public void LoadFromFile_ValidFileJapanese_ReturnsInternalTextualMedia()
        {
            string path = "test_japanese.txt";
            string testContent = "こんにちは世界"; // "Hello World" in Japanese
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }


        [TestMethod]
        public void LoadFromFile_ValidFileRussian_ReturnsInternalTextualMedia()
        {
            string path = "test_russian.txt";
            string testContent = "Привет мир"; // "Hello World" in Russian
            File.WriteAllText(path, testContent);

            TextualMedia result = loader.ImportFromFile(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(testContent, result.Text);
        }
    }

}
﻿using NUnit.Framework;
using System.Configuration;
using System.Linq;

namespace YandexLinguistics.NET.Tests
{
	[TestFixture]
	public class TranslatorTests
	{
		Translator Translator;

		[SetUp]
		public void Init()
		{
			Translator = new Translator(ConfigurationManager.AppSettings["TranslatorKey"]);
		}

		[Test]
		public void TranslatorGetLangs()
		{
			LangPair[] expectedLangPairs = Translator.GetLangs();
			var langPairs = typeof(TranslatorDirectory).GetFields()
				.Where(field => field.FieldType == typeof(LangPair))
				.Select(field => (LangPair)field.GetValue(null));

			CollectionAssert.AreEquivalent(expectedLangPairs, langPairs);
		}

		[Test]
		public void TranslatorDetectLang()
		{
			var lang = Translator.DetectLang("Язик до Києва доведе");
			Assert.AreEqual(Lang.Uk, lang);
		}

		[Test]
		public void TranslatorTranslate()
		{
			var translation = Translator.Translate("Лучше поздно, чем никогда", TranslatorDirectory.EnRu);
			Assert.AreEqual("Лучше поздно, чем никогда", translation.Text);
			Assert.AreEqual(TranslatorDirectory.EnRu, translation.LangPair);
		}

		[Test]
		public void TranslatorDetectAndTranslate()
		{
			var translation = Translator.Translate("Семь раз отмерь, один раз отрежь", new LangPair(Lang.None, Lang.En), null, true);
			Assert.AreEqual("Measure twice, cut once", translation.Text);
			Assert.AreEqual(TranslatorDirectory.RuEn, translation.LangPair);
			Assert.AreEqual(Lang.Ru, translation.Detected.Lang);
		}

		[Test]
		public void TranslatorLangNotSupported()
		{
			var exception = Assert.Throws<YandexLinguisticsException>(
				() => Translator.Translate("Example text", new LangPair(Lang.None, Lang.None)));
			Assert.AreEqual(
				new YandexLinguisticsException(502, "Invalid parameter: lang").ToString(),
				exception.ToString());
		}

		[Test]
		public void TranslatorVeryLongInputString()
		{
			var exception = Assert.Throws<YandexLinguisticsException>(
				() => Translator.Translate(new string('a', 100000), new LangPair(Lang.None, Lang.Ru)));
			Assert.AreEqual(
				new YandexLinguisticsException(0, "Invalid URI: The Uri string is too long.").ToString(),
				exception.ToString());
		}
	}
}
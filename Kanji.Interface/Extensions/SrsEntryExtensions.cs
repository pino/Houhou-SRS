﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Helpers;

namespace Kanji.Interface.Extensions
{
    static class SrsEntryExtensions
    {
        /// <summary>
        /// Loads properties from the given kanji entity.
        /// </summary>
        /// <param name="se">Target SRS entry.</param>
        /// <param name="k">Kanji to load.</param>
        public static void LoadFromKanji(this SrsEntry se, KanjiEntity k)
        {
            var vocabs = new VocabDao().GetFilteredVocab(k, null, null, null, 6, 0, true, true, wikiOnly: true).ToArray();

            // Compute the meaning string.
            string meaningString = string.Empty;
            foreach (KanjiMeaning km in k.Meanings)
            {
                meaningString += MultiValueFieldHelper.ReplaceSeparator(km.Meaning) + MultiValueFieldHelper.ValueSeparator;
            }
            meaningString = meaningString.Trim(
                new char[] { MultiValueFieldHelper.ValueSeparator });
            meaningString = MultiValueFieldHelper.Expand(meaningString);

            // Compute the reading string.
            string readingString = (k.OnYomi
                + MultiValueFieldHelper.ValueSeparator)
                + string.Join(" ", k.KunYomi.Split(',').Select(kun => kun.Replace("-", "").Split('.').FirstOrDefault()));
            readingString = MultiValueFieldHelper.Expand(readingString
                .Trim(new char[] { MultiValueFieldHelper.ValueSeparator })).Replace(", ", " ");
            // speedrun - use only detected in vocab
            var readings = readingString.Split(' ').Distinct();
            var furiganas = vocabs.SelectMany(v => v.Furigana.Split(';').Where(f => f.Split(':').FirstOrDefault() == v.KanjiWriting.IndexOf(k.Character).ToString()).Select(f => f.Split(':').LastOrDefault())).Distinct();
            readings = readings.Where(r => furiganas.Contains(r)).ToArray();
            readingString = string.Join(" ", readings);

            // Set values.
            se.Meanings = meaningString;
            se.Readings = readingString;
            se.AssociatedKanji = k.Character;
        }

        /// <summary>
        /// Loads properties from the given vocab entity.
        /// </summary>
        /// <param name="se">Target SRS entry.</param>
        /// <param name="v">Vocab to load.</param>
        public static void LoadFromVocab(this SrsEntry se, VocabEntity v)
        {
            // Compute the meaning string.
            string meaningString = string.Empty;
            foreach (VocabMeaning vm in v.Meanings)
            {
                meaningString += MultiValueFieldHelper.ReplaceSeparator(vm.Meaning)
                    .Replace(" ;", MultiValueFieldHelper.ValueSeparator.ToString())
                    + MultiValueFieldHelper.ValueSeparator;
            }
            meaningString = meaningString.Trim(
                new char[] { MultiValueFieldHelper.ValueSeparator });
            meaningString = MultiValueFieldHelper.Trim(meaningString);
            meaningString = MultiValueFieldHelper.Expand(meaningString);

            // Set values.
            se.Meanings = meaningString;
            se.Readings = v.KanaWriting;
            se.AssociatedVocab = v.KanjiWriting;

            if (string.IsNullOrEmpty(se.AssociatedVocab))
            {
                se.AssociatedVocab = v.KanaWriting;
            }
        }

        /// <summary>
        /// Builds and returns a clone of this instance.
        /// </summary>
        /// <returns>Clone of this instance.</returns>
        public static SrsEntry Clone(this SrsEntry se)
        {
            return new SrsEntry()
                {
                    AssociatedKanji = se.AssociatedKanji,
                    AssociatedVocab = se.AssociatedVocab,
                    CreationDate = se.CreationDate,
                    CurrentGrade = se.CurrentGrade,
                    FailureCount = se.FailureCount,
                    ID = se.ID,
                    SuspensionDate = se.SuspensionDate,
                    MeaningNote = se.MeaningNote,
                    Meanings = MultiValueFieldHelper.Expand(se.Meanings),
                    NextAnswerDate = se.NextAnswerDate,
                    ReadingNote = se.ReadingNote,
                    Readings = MultiValueFieldHelper.Expand(se.Readings),
                    SuccessCount = se.SuccessCount,
                    Tags = MultiValueFieldHelper.Expand(se.Tags)
                };
        }
    }
}

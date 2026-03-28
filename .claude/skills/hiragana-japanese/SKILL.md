---
name: hiragana-japanese
description: >
  Japanese Hiragana character and word reference for game development. Use this skill
  whenever the user needs Japanese text for UI, menus, dialogue, item names, ability names,
  or any in-game text written in Hiragana. Also trigger when the user asks for Japanese
  translations, romanization, pronunciation, Hiragana character sets, or needs help
  choosing Japanese words for game elements. This skill covers ONLY Hiragana -- not
  Katakana, not Kanji. If the user needs Katakana or Kanji, note the limitation and
  provide Hiragana alternatives. Useful for rhythm game UI, VR game menus, tutorial text,
  score screens, and thematic flavor text. Always pair Hiragana with romaji for the
  development team.
---

# Japanese Hiragana for Game Development

This skill provides Hiragana characters and vocabulary for use in game UI, menus, dialogue, and thematic text. All output is Hiragana only (no Katakana, no Kanji) with romaji transliteration for the development team.

## When to Read Reference Files

| Topic | File | When |
|-------|------|------|
| Full Hiragana chart | `references/hiragana-chart.md` | Character lookup, font testing |
| Game vocabulary | `references/game-vocabulary.md` | UI labels, menu items, ability names |
| Grammar basics | `references/grammar-basics.md` | Dialogue lines, tutorial text |

---

## Hiragana at a Glance

Hiragana is one of three Japanese writing systems. It represents syllables (not individual letters). There are 46 basic characters, plus modified forms (dakuten/handakuten) and combinations.

Each character = one syllable. Japanese has a very regular pronunciation system -- each character is always pronounced the same way.

### Vowels
| Hiragana | Romaji | Sound |
|----------|--------|-------|
| あ | a | "ah" as in "father" |
| い | i | "ee" as in "feet" |
| う | u | "oo" as in "food" |
| え | e | "eh" as in "pet" |
| お | o | "oh" as in "go" |

### Consonant Rows (first character of each row)
| Row | Characters | Romaji |
|-----|-----------|--------|
| か行 | か き く け こ | ka ki ku ke ko |
| さ行 | さ し す せ そ | sa shi su se so |
| た行 | た ち つ て と | ta chi tsu te to |
| な行 | な に ぬ ね の | na ni nu ne no |
| は行 | は ひ ふ へ ほ | ha hi fu he ho |
| ま行 | ま み む め も | ma mi mu me mo |
| や行 | や ゆ よ | ya yu yo |
| ら行 | ら り る れ ろ | ra ri ru re ro |
| わ行 | わ を | wa wo |
| ん | ん | n |

### Dakuten (Voiced) -- add ゛
| Base | Voiced | Romaji |
|------|--------|--------|
| か -> | が | ga |
| さ -> | ざ | za |
| た -> | だ | da |
| は -> | ば | ba |

### Handakuten (Half-voiced) -- add ゜
| Base | Half-voiced | Romaji |
|------|------------|--------|
| は -> | ぱ | pa |

## Unity Font Setup for Japanese

1. Import a font that supports Japanese characters (e.g., Noto Sans JP, M PLUS Rounded).
2. If using TextMeshPro:
   - Create a TMP Font Asset from the Japanese font.
   - In the Font Asset Creator, set Character Set to **Custom Characters**.
   - Paste all Hiragana characters you need into the custom character field.
   - Generate the atlas.
3. If using UI Toolkit:
   - Add the font as a USS font-family.
   - Ensure the font file includes Hiragana glyphs.

The reference file `references/hiragana-chart.md` contains all characters in a copy-paste-ready block for font atlas generation.

## Naming Guidelines for Games

When creating Japanese names for game elements:
- Keep words short (2-4 syllables) for readability in VR.
- Always provide romaji alongside Hiragana in code comments and documentation.
- Test that your chosen font renders every character correctly at the target size.
- In VR, larger text sizes (at least 24pt equivalent) are needed for legibility.
- Consider adding furigana-style romaji below Hiragana for non-Japanese-reading players.

## Quick Reference: Common Game Words

| English | Hiragana | Romaji |
|---------|----------|--------|
| Start | はじめ | hajime |
| End | おわり | owari |
| Fire | ひ | hi |
| Water | みず | mizu |
| Rock | いわ | iwa |
| Sword | かたな | katana |
| Hit | あたり | atari |
| Miss | はずれ | hazure |
| Ready | よい | yoi |
| Go | いけ | ike |
| Score | てんすう | tensuu |
| Time | じかん | jikan |

See `references/game-vocabulary.md` for the full list organized by category.

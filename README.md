# Darkest Night Reforjed

Port of the Darkest Night event to RimWorld 1.6 without Harmony. It uses Rework Reforjed for the sky tint.

## What it does

- Incident `Darkness`: a dark, cold game condition falls on your home map, with countdown messages and a hidden counter building (`BVcounter`) that spawns Darkspawn.
- Permanent scenario `Permanent Darkness`: endless night with constant spawns. Daylight can burn the spawn away from the mod settings.
- Darkspawn (`Void`): fast animal pawn with its own think tree. It picks up corpses, smashes power buildings and doors, and hunts humanlike pawns in the dark. Light kills it.
- Sky stays dark through four Rework redirects (`SkyColorSet.Lerp/LerpDarken`, `SkyTarget.Lerp/LerpDarken`), only while the permanent condition is active.

## Requirements

- RimWorld 1.6.
- Rework Reforjed active and loaded before this mod.

## Install

Copy the `Darkest Night Reforjed` folder into `RimWorld\Mods\`, enable Rework Reforjed first and this mod after it.

## Settings

Base chance, refire days, earliest day, daylight burn, narrator mute, ignore downed pawns.

## Notes

- Defs, textures and sounds come from the 1.4 release. Code was recompiled against 1.6 and the three Harmony patches were replaced by Rework redirects.
- The cell inspector groups repeated rows ("Darkspawn x20") instead of listing them one by one.
- Korean and Russian translations are kept as they were. English was rewritten, Spanish fixed to UTF-8.

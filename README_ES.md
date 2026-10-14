# Darkest Night Reforjed

Port del evento Darkest Night a RimWorld 1.6 sin Harmony. El tinte del cielo lo pone Rework Reforjed.

## Qué hace

- Incidente `Darkness`: cae una condición de oscuridad fría sobre tu mapa, con cuenta atrás y un contador oculto (`BVcounter`) que genera Darkspawns.
- Escenario `Permanent Darkness`: noche sin fin con apariciones constantes. Desde ajustes la luz del día puede quemar las apariciones.
- Darkspawn (`Void`): pawn animal rápido con su propio think tree. Recoge cadáveres, rompe generadores y puertas, y caza peones humanoides a oscuras. La luz lo mata.
- El cielo se mantiene oscuro con cuatro redirects de Rework (`SkyColorSet.Lerp/LerpDarken`, `SkyTarget.Lerp/LerpDarken`), solo mientras la condición permanente está activa.

## Requisitos

- RimWorld 1.6.
- Rework Reforjed activo y cargado antes que este mod.

## Instalación

Copia la carpeta `Darkest Night Reforjed` en `RimWorld\Mods\`, activa Rework Reforjed primero y este mod después.

## Ajustes

Probabilidad base, días de espera, primer día, quemado por luz solar, silenciar narrador, ignorar derribados.

## Notas

- Los Defs, texturas y sonidos vienen de la versión 1.4. El código se recompiló contra 1.6 y los tres parches Harmony se cambiaron por redirects de Rework.
- El inspector de celda agrupa las filas repetidas ("Peón oscuro x20") en vez de listarlas una por una.
- Las traducciones coreana y rusa se dejan como estaban. La inglesa se reescribió y la española se pasó a UTF-8.

**NO LONGER MAINTAINED DUE TO CONSTANT UNITY UPDATES TO HLSL CODE. FEEL FREE TO BRANCH/COPY CODE, JUST MAKE SURE THAT ORIGINAL AUTHOR OF TEXEL MATH (https://forum.unity.com/members/greatestbear.1727336/) IS MENTIONED.**

This is getting out of hand. Unity is a mess. God knows I've tried to support unity as a c# programmer till the end. Fare thee well, my friends and may you find yourself more patience!

**HEAVY TESTING REQUIRED! USE AT YOUR OWN RISK!**

**DO NOT USE TERRAIN SHADER! DUE TO UNITY HANDLING TERRAIN CONTROL TEXTURES, VISIBLE NORMAL\ALBEDO MISMATCH. CURRENTLY WIP. TERRAIN SHADER MAY BE EXCLUDED FROM PACKAGE LATER**

# UnityTexelShaders
URP Texel Lighting

Based on *com.unity.render-pipelines.universal@7.3.1* package / Lit shader

Actual texel calculation shader math belongs to https://forum.unity.com/members/greatestbear.1727336/

Inspired by this Unity forum thread: https://forum.unity.com/threads/the-quest-for-efficient-per-texel-lighting.529948/

The aim of this project is to make late-1990-early-2000 game graphics, but with all-new look. Think of it as of half-life 1 software renderer with pixelated PBR.

This will not instantly make you game look better (older), as it requires a pixel-prefect texuring and a hard 3d work on low-poly topography\UV mapping.

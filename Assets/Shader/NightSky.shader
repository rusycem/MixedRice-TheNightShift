Shader "Custom/NightSky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.05, 0.05, 0.15, 1)
        _HorizonColor ("Horizon Color", Color) = (0.1, 0.05, 0.2, 1)
        _BottomColor ("Bottom Color", Color) = (0.02, 0.02, 0.08, 1)
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1.5
        _StarDensity ("Star Density", Range(0, 1)) = 0.3
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1.2
        _StarSize ("Star Size", Range(0, 1)) = 0.05
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 2
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _GradientPower;
            float _StarDensity;
            float _StarBrightness;
            float _StarSize;
            float _FlickerSpeed;
            float _FlickerAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(lerp(hash(p + float3(0,0,0)), hash(p + float3(1,0,0)), f.x),
                         lerp(hash(p + float3(0,1,0)), hash(p + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(p + float3(0,0,1)), hash(p + float3(1,0,1)), f.x),
                         lerp(hash(p + float3(0,1,1)), hash(p + float3(1,1,1)), f.x), f.y), f.z);
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                float y = dir.y;

                float4 skyColor;
                if (y > 0)
                {
                    skyColor = lerp(_HorizonColor, _TopColor, pow(y, _GradientPower));
                }
                else
                {
                    skyColor = lerp(_HorizonColor, _BottomColor, pow(-y, _GradientPower));
                }

                float3 starPos = dir * 100.0;
                float starNoise = hash(floor(starPos));

                float star = 0.0;
                if (starNoise > (1.0 - _StarDensity))
                {
                    float3 cellPos = frac(starPos);
                    float dist = length(cellPos - 0.5);
                    star = smoothstep(_StarSize, 0.0, dist);

                    float flicker = noise(float3(starPos.xy * 0.1, _Time.y * _FlickerSpeed));
                    flicker = lerp(1.0 - _FlickerAmount, 1.0, flicker);
                    star *= flicker;

                    star *= _StarBrightness;
                }

                float4 finalColor = skyColor + float4(star, star, star, 0);

                return finalColor;
            }
            ENDCG
        }
    }
}

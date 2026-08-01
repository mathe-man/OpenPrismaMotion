layout(location = 0) in vec2 aLocal;
layout(location = 1) in vec4 aStartEnd;

uniform mat4 uMvp;
uniform vec3 uCameraPosition;
uniform float uZStart; // starting frame depth
uniform float uZEnd;   // next frame depth
uniform float uThickness;

out float vAlongLength;

void main()
{
    vec3 start3D = vec3(aStartEnd.xy, uZStart);
    vec3 end3D = vec3(aStartEnd.zw, uZEnd);
    vec3 dir3D = end3D - start3D;
    float len = length(dir3D);
    vec3 dirN = len > 0.0001 ? dir3D / len : vec3(1.0, 0.0, 0.0);

    vec3 point = mix(start3D, end3D, aLocal.x);
    vec3 viewDir = normalize(uCameraPosition - point);


    vec3 perp = normalize(cross(dirN, viewDir));

    vec3 worldPos = point + perp * (aLocal.y * uThickness);
    gl_Position = uMvp * vec4(worldPos, 1.0);
    vAlongLength = aLocal.x;
}
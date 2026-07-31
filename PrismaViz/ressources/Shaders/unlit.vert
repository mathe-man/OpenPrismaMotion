layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;
layout(location = 2) in vec2 aUV;

out vec4 vColor;
out vec2 vUV;

// Model View Projection
uniform mat4 uMvp;

void main()
{
    gl_Position = uMvp * vec4(aPosition, 1.0);
    vColor = aColor;
    vUV = aUV;
}
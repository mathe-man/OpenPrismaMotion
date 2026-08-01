in vec4 vColor;
in vec2 vUV;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, vUV) * vColor;
}
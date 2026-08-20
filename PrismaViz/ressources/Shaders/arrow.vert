layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aFlow;

uniform mat4 uMvp;
uniform float uMinMagnitude;

uniform float uZStart;
uniform float uZEnd;

void main()
{
    

    float magnitude = length(aFlow);

    if (magnitude < uMinMagnitude)
    {
        gl_Position = vec4(2.0, 2.0, 0.0, 1.0);
        return;
    }
    

    // This vertex is a start
    if (gl_VertexID % 2 == 0) {
        gl_Position = uMvp * vec4(aPosition, uZStart, 1.0);
    }

    // This vertex is an end
    else if (gl_VertexID % 2 == 1) {
        // The position already take care of the flow movement
        gl_Position = uMvp * vec4(aPosition, uZEnd, 1.0);
    }
}
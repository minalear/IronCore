#version 400
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aUV;

out VERTEX_INFO {
    vec2 UV;
} vs_out;

uniform mat4 proj;
uniform mat4 view;
uniform mat4 model;
uniform vec4 source;

void main() {
    vs_out.UV = aUV * source.zw + source.xy;
    gl_Position = proj * view * model * vec4(aPos, 0.0, 1.0);
}
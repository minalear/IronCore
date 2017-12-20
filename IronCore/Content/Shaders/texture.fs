#version 400
in VERTEX_INFO {
    vec2 UV;
} fs_in;

out vec4 fragmentColor;

uniform sampler2D texture;

void main() {
    fragmentColor = texture2D(texture, fs_in.UV);
}
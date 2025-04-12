#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec3 vNormal;

uniform mat4 uModel;
uniform mat3 uNormal;
uniform mat4 uView;
uniform mat4 uProjection;
uniform bool uUsePrependicularNormals;

out vec4 outCol;
out vec3 outNormal;
out vec3 outWorldPosition;
        
void main()
{
	outCol = vCol;
    if (uUsePrependicularNormals)
    {
        outNormal = uNormal*vNormal;
    }
    else
    {
       float angle = radians(10);
        mat3 rotationMatrix = mat3(
            vec3(cos(angle), 0, sin(angle)),
            vec3(0, 1, 0),
            vec3(-sin(angle), 0, cos(angle))
        );
        outNormal = rotationMatrix * vNormal; 
    }
    outWorldPosition = vec3(uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0));
    gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
}
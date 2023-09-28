shader_type spatial;
render_mode depth_test_disabled, depth_draw_never, cull_front, unshaded, skip_vertex_transform, alpha_to_coverage_and_one;

uniform sampler2D depth_texture : hint_depth_texture;

varying mat4 inv_matrix;

void vertex()
{
	float scale = length(MODEL_MATRIX[0]);
	vec3 vertex = MODEL_MATRIX[3].xyz;
	
	mat4 matrix = mat4(
        vec4(scale, 0.0, 0.0, 0.0),
        vec4(0.0, scale, 0.0, 0.0),
        vec4(0.0, 0.0, scale, 0.0),
        vec4(vertex, 1.0)
	);
	
	inv_matrix = inverse(matrix);
	
	VERTEX = (VIEW_MATRIX * (matrix * vec4(VERTEX, 1.0f))).xyz;
}

void fragment()
{
	// Sample depth at screen uv
	float depth = textureLod(depth_texture, SCREEN_UV, 0.0f).r;

	// Convert to world coordinate
	vec3 ndc = vec3(SCREEN_UV * 2.0f - 1.0f, depth);
	vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	vec3 world_position = world.xyz / world.w;
	
	// Get position against model matrix
	world_position = (inv_matrix * vec4(world_position, 1.0f)).xyz; //(world_position + inv_vertex) * inv_scale;
	
	// Check if point is in shadow
    float y_alpha = max(world_position.y, 0.0f); // world_position.y * step(-world_position.y, 0.0f);
	float y_alpha_s = 1.0f - y_alpha * y_alpha;
	y_alpha = sqrt(y_alpha_s);
	
    float radius = (1.0f + min(world_position.y, 0.0f) / 128.0f) * y_alpha;
	
	float alpha = (1.0f - step(radius, length(world_position.xz)));
	alpha *= y_alpha_s * 0.5f;

    ALBEDO = vec3(0.0, 0.0, 0.0);
    ALPHA = alpha;
}

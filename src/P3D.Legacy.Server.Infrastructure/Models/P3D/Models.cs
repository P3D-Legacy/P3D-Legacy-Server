using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.Infrastructure.Models.P3D;

internal record ReasonDTO(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt);

internal record UserDTO(
    [property: JsonPropertyName("id")] ulong Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("profile_photo_url")] string ProfilePhotoUrl,
    [property: JsonPropertyName("roles")] IReadOnlyList<RoleDTO>? Roles,
    [property: JsonPropertyName("gamejolt")] GameJoltDTO? GameJolt,
    [property: JsonPropertyName("forum")] ForumDTO? UserForum);

internal record GameJoltDTO(
    [property: JsonPropertyName("id")] ulong Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("verified_at")] DateTime VerifiedAt,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("deleted_at")] DateTime? DeletedAt,
    [property: JsonPropertyName("user")] UserDTO? User);

internal record ForumDTO(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("verified_at")] DateTime VerifiedAt,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("deleted_at")] DateTime? DeletedAt,
    [property: JsonPropertyName("user")] UserDTO? User);

internal record RoleDTO(
    [property: JsonPropertyName("id")] ulong Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("permissions")] IReadOnlyList<PermissionDTO> Permissions);

internal record PermissionDTO(
    [property: JsonPropertyName("id")] ulong Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt);
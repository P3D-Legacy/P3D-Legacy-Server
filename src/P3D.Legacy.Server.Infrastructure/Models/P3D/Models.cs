using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Infrastructure.Models.P3D
{
    internal record UserDTO(
        [property: JsonProperty("id")] ulong Id,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("email")] string Email,
        [property: JsonProperty("username")] string Username,
        [property: JsonProperty("created_at")] DateTime CreatedAt,
        [property: JsonProperty("updated_at")] DateTime UpdatedAt,
        [property: JsonProperty("profile_photo_url")] string ProfilePhotoUrl,
        [property: JsonProperty("roles")] IReadOnlyList<RoleDTO>? Roles);

    internal record GameJoltDTO(
        [property: JsonProperty("id")] ulong Id,
        [property: JsonProperty("username")] string Username,
        [property: JsonProperty("verified_at")] DateTime VerifiedAt,
        [property: JsonProperty("created_at")] DateTime CreatedAt,
        [property: JsonProperty("updated_at")] DateTime UpdatedAt,
        [property: JsonProperty("deleted_at")] DateTime? DeletedAt);

    internal record ForumDTO(
        [property: JsonProperty("username")] string Username,
        [property: JsonProperty("verified_at")] DateTime VerifiedAt,
        [property: JsonProperty("created_at")] DateTime CreatedAt,
        [property: JsonProperty("updated_at")] DateTime UpdatedAt,
        [property: JsonProperty("deleted_at")] DateTime? DeletedAt);

    internal record RoleDTO(
        [property: JsonProperty("id")] ulong Id,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("created_at")] DateTime CreatedAt,
        [property: JsonProperty("updated_at")] DateTime UpdatedAt,
        [property: JsonProperty("permissions")] IReadOnlyList<PermissionDTO> Permissions);

    internal record PermissionDTO(
        [property: JsonProperty("id")] ulong Id,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("created_at")] DateTime CreatedAt,
        [property: JsonProperty("updated_at")] DateTime UpdatedAt);
}
﻿namespace FlashTeams.Shared.Dtos.Users;

public class UserUpdateDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Password { get; set; }

    public string Username { get; set; }
}
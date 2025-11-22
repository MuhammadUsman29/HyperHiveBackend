using System;
using System.Threading.Tasks;
using BCrypt.Net;
using HyperHiveBackend.DataAccess;
using HyperHiveBackend.Models;

namespace HyperHiveBackend.Services;

public interface IUserService
{
    Task<UserDto?> SignupAsync(SignupRequest request);
    Task<User?> GetUserByEmailAsync(string email);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}

public class UserService : IUserService
{
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IDbRepository dbRepository, ILogger<UserService> logger)
    {
        _dbRepository = dbRepository;
        _logger = logger;
    }

    public async Task<UserDto?> SignupAsync(SignupRequest request)
    {
        // Check if user already exists
        var existingUser = await GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var passwordHash = HashPassword(request.Password);

        // Insert user
        var insertQuery = @"
            INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt, UpdatedAt, IsActive)
            VALUES (@Email, @PasswordHash, @FirstName, @LastName, @CreatedAt, @UpdatedAt, @IsActive);
            SELECT LAST_INSERT_ID();";

        var now = DateTime.UtcNow;
        var userId = await _dbRepository.QuerySingleOrDefaultAsync<int>(insertQuery, new
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        });

        // Get created user
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("Failed to create user");
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var query = @"
            SELECT Id, Email, PasswordHash, FirstName, LastName, CreatedAt, UpdatedAt, IsActive
            FROM Users
            WHERE Email = @Email AND IsActive = TRUE
            LIMIT 1";

        return await _dbRepository.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
    }

    private async Task<User?> GetUserByIdAsync(int id)
    {
        var query = @"
            SELECT Id, Email, PasswordHash, FirstName, LastName, CreatedAt, UpdatedAt, IsActive
            FROM Users
            WHERE Id = @Id AND IsActive = TRUE
            LIMIT 1";

        return await _dbRepository.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }
}


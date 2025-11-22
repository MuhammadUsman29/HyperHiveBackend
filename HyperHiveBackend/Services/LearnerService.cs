using HyperHiveBackend.Data;
using HyperHiveBackend.DTOs;
using HyperHiveBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HyperHiveBackend.Services
{
    public interface ILearnerService
    {
        Task<LearnerResponse> CreateLearnerAsync(CreateLearnerRequest request);
        Task<LearnerResponse> UpdateLearnerAsync(int id, UpdateLearnerRequest request);
        Task<LearnerResponse?> GetLearnerByIdAsync(int id);
        Task<LearnerResponse?> GetLearnerByEmailAsync(string email);
        Task<LearnersListResponse> GetAllLearnersAsync(int page = 1, int pageSize = 10);
        Task<bool> DeleteLearnerAsync(int id);
    }

    public class LearnerService : ILearnerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LearnerService> _logger;

        public LearnerService(ApplicationDbContext context, ILogger<LearnerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LearnerResponse> CreateLearnerAsync(CreateLearnerRequest request)
        {
            // Check if email already exists
            var existingLearner = await _context.Learners
                .FirstOrDefaultAsync(l => l.Email == request.Email);

            if (existingLearner != null)
            {
                throw new Exception($"Learner with email {request.Email} already exists");
            }

            var learner = new Learner
            {
                Name = request.Name,
                Email = request.Email,
                Position = request.Position,
                Department = request.Department,
                JoinedDate = request.JoinedDate,
                Bio = request.Bio,
                AIProfileData = request.AIProfile != null 
                    ? JsonSerializer.Serialize(request.AIProfile) 
                    : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Learners.Add(learner);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created learner with ID {LearnerId}", learner.Id);

            return MapToResponse(learner);
        }

        public async Task<LearnerResponse> UpdateLearnerAsync(int id, UpdateLearnerRequest request)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
            {
                throw new Exception($"Learner with ID {id} not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Name))
                learner.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                // Check if new email is already taken by another learner
                var existingLearner = await _context.Learners
                    .FirstOrDefaultAsync(l => l.Email == request.Email && l.Id != id);
                
                if (existingLearner != null)
                {
                    throw new Exception($"Email {request.Email} is already taken");
                }
                
                learner.Email = request.Email;
            }
            
            if (!string.IsNullOrEmpty(request.Position))
                learner.Position = request.Position;
            
            if (!string.IsNullOrEmpty(request.Department))
                learner.Department = request.Department;
            
            if (request.JoinedDate.HasValue)
                learner.JoinedDate = request.JoinedDate.Value;
            
            if (request.Bio != null)
                learner.Bio = request.Bio;
            
            if (request.AIProfile != null)
                learner.AIProfileData = JsonSerializer.Serialize(request.AIProfile);

            learner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated learner with ID {LearnerId}", learner.Id);

            return MapToResponse(learner);
        }

        public async Task<LearnerResponse?> GetLearnerByIdAsync(int id)
        {
            var learner = await _context.Learners.FindAsync(id);
            return learner != null ? MapToResponse(learner) : null;
        }

        public async Task<LearnerResponse?> GetLearnerByEmailAsync(string email)
        {
            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.Email == email);
            
            return learner != null ? MapToResponse(learner) : null;
        }

        public async Task<LearnersListResponse> GetAllLearnersAsync(int page = 1, int pageSize = 10)
        {
            var totalCount = await _context.Learners.CountAsync();
            
            var learners = await _context.Learners
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new LearnersListResponse
            {
                Learners = learners.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        public async Task<bool> DeleteLearnerAsync(int id)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
            {
                return false;
            }

            _context.Learners.Remove(learner);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted learner with ID {LearnerId}", id);

            return true;
        }

        private LearnerResponse MapToResponse(Learner learner)
        {
            LearnerAIProfile? aiProfile = null;
            
            if (!string.IsNullOrEmpty(learner.AIProfileData))
            {
                try
                {
                    aiProfile = JsonSerializer.Deserialize<LearnerAIProfile>(learner.AIProfileData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize AI profile for learner {LearnerId}", learner.Id);
                }
            }

            return new LearnerResponse
            {
                Id = learner.Id,
                Name = learner.Name,
                Email = learner.Email,
                Position = learner.Position,
                Department = learner.Department,
                JoinedDate = learner.JoinedDate,
                Bio = learner.Bio,
                AIProfile = aiProfile,
                CreatedAt = learner.CreatedAt,
                UpdatedAt = learner.UpdatedAt
            };
        }
    }
}


﻿using CommUnity.Shared.DTOs;
using CommUnity.Shared.Entities;
using CommUnity.Shared.Responses;

namespace CommUnity.BackEnd.Repositories.Interfaces
{
    public interface IResidentialUnitRepository
    {
        Task<ActionResponse<ResidentialUnit>> GetAsync(int id);

        Task<ActionResponse<IEnumerable<ResidentialUnit>>> GetAsync();

        Task<ActionResponse<IEnumerable<ResidentialUnit>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalPagesAsync(PaginationDTO pagination);

        Task<ActionResponse<IEnumerable<ResidentialUnit>>> GetFullAsync(int id);

        Task<ActionResponse<int>> GetRecordsNumber(PaginationDTO pagination);
    }
}

﻿using CommUnity.Shared.DTOs;
using CommUnity.Shared.Entities;
using CommUnity.Shared.Responses;

namespace CommUnity.BackEnd.UnitsOfWork.Interfaces
{
    public interface IApartmentsUnitOfWork
    {
        Task<ActionResponse<Apartment>> GetAsync(int id);

        Task<ActionResponse<IEnumerable<Apartment>>> GetAsync();

        Task<ActionResponse<IEnumerable<Apartment>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalPagesAsync(PaginationDTO pagination);

        Task<ActionResponse<IEnumerable<Apartment>>> GetFullAsync(int id);

        Task<ActionResponse<int>> GetRecordsNumber(PaginationDTO pagination);
    }
}

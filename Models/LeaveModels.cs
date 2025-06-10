using System;
using System.Collections.Generic;

namespace WiseHR.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string EmployeeID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string MentorName { get; set; }
        public string MentorEmail { get; set; }
    }

    public class LeaveHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string EmployeeID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string MentorName { get; set; }
        public string MentorEmail { get; set; }
        public string ApproverId { get; set; }
        public string ApproverName { get; set; }
        public string ApproverEmail { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; }
    }

    public class UserLeaveBalance
    {
        public string UserId { get; set; }
        public string EmployeeID { get; set; }
        public string LeaveType { get; set; }
        public int TotalQuota { get; set; }
        public int Consumed { get; set; }
        public int Available { get; set; }
    }

    public class LeaveRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string LeaveType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
        public string MentorName { get; set; }
        public string MentorEmail { get; set; }
    }

    public class ApproveRejectDto
    {
        public int LeaveRequestId { get; set; }
        public bool IsApproved { get; set; }
    }

    public class DefaultLeaveQuotaDto
    {
        public string LeaveType { get; set; }
        public int Quota { get; set; }
        public string Description { get; set; }
    }

    public class IndividualLeaveQuotaDto
    {
        public string EmployeeID { get; set; }
        public string LeaveType { get; set; }
        public int Quota { get; set; }
        public string Description { get; set; }
    }

    public class UserHistoryRequestDto
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string EmployeeId { get; set; }
        public string Email { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SubmitLeaveResponse
    {
        public string Message { get; set; }
        public List<LeaveRequestSummary> LeaveRequests { get; set; }
        public PreviousStats PreviousStats { get; set; }
    }

    public class LeaveRequestSummary
    {
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }

    public class PreviousStats
    {
        public string LeaveType { get; set; }
        public int TotalQuota { get; set; }
        public int Consumed { get; set; }
        public int Available { get; set; }
    }

    public class MyHistoryResponse
    {
        public List<LeaveHistory> History { get; set; }
        public List<UserLeaveBalance> Balances { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class ApproveRejectResponse
    {
        public string Message { get; set; }
    }

    public class QuotaResponse
    {
        public string Message { get; set; }
    }

    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
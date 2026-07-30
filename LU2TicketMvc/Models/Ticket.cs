namespace LU2TicketMvc.Models;
public class Ticket
{
    public int Id { get; set; } //Stores the unique ticket number
    public string StudentName { get; set; } = ""; //Stores the student name.
    public string StudentEmail { get; set; } = ""; //Stores the student email

    public string Issue { get; set; } = ""; //Stores the support issue

    public string Status { get; set; } = "Open"; //Stores the current ticket status
}


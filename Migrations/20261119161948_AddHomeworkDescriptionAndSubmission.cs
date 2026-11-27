using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepByStep.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkDescriptionAndSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC2C8A1EB4", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Parents__D339510F0D8996D2", x => x.ParentID);
                    table.ForeignKey(
                        name: "FK__Parents__ParentI__3C69FB99",
                        column: x => x.ParentID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Teachers__EDF25944EB96F8E0", x => x.TeacherID);
                    table.ForeignKey(
                        name: "FK__Teachers__Teache__3F466844",
                        column: x => x.TeacherID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E32C3012057", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notificat__Paren__59FA5E80",
                        column: x => x.ParentID,
                        principalTable: "Parents",
                        principalColumn: "ParentID");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Students__32C52A79D20F4F70", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK__Students__Parent__4316F928",
                        column: x => x.ParentID,
                        principalTable: "Parents",
                        principalColumn: "ParentID");
                    table.ForeignKey(
                        name: "FK__Students__Studen__4222D4EF",
                        column: x => x.StudentID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Subjects__AC1BA388397EA82F", x => x.SubjectID);
                    table.ForeignKey(
                        name: "FK__Subjects__Teache__45F365D3",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID");
                });

            migrationBuilder.CreateTable(
                name: "Attendance_Homework",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))"),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    HomeworkCompleted = table.Column<bool>(type: "bit", nullable: false),
                    HomeworkDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attendan__FBDF78C91CDA73C0", x => x.RecordID);
                    table.ForeignKey(
                        name: "FK__Attendanc__Stude__4F7CD00D",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                    table.ForeignKey(
                        name: "FK__Attendanc__Subje__5070F446",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID");
                });

            migrationBuilder.CreateTable(
                name: "Study_Plans",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    RecommendedHours = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    PriorityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Study_Pl__755C22D704DEE6E7", x => x.PlanID);
                    table.ForeignKey(
                        name: "FK__Study_Pla__Stude__5441852A",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                    table.ForeignKey(
                        name: "FK__Study_Pla__Subje__5535A963",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID");
                });

            migrationBuilder.CreateTable(
                name: "Weekly_Evaluations",
                columns: table => new
                {
                    EvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    TeacherNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EvaluationDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Weekly_E__36AE68D3B41D5031", x => x.EvaluationID);
                    table.ForeignKey(
                        name: "FK__Weekly_Ev__Stude__4AB81AF0",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                    table.ForeignKey(
                        name: "FK__Weekly_Ev__Subje__4BAC3F29",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Homework_StudentID",
                table: "Attendance_Homework",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Homework_SubjectID",
                table: "Attendance_Homework",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ParentID",
                table: "Notifications",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentID",
                table: "Students",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_Study_Plans_StudentID",
                table: "Study_Plans",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Study_Plans_SubjectID",
                table: "Study_Plans",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_TeacherID",
                table: "Subjects",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053446845606",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Weekly_Evaluations_StudentID",
                table: "Weekly_Evaluations",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Weekly_Evaluations_SubjectID",
                table: "Weekly_Evaluations",
                column: "SubjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance_Homework");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Study_Plans");

            migrationBuilder.DropTable(
                name: "Weekly_Evaluations");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Parents");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

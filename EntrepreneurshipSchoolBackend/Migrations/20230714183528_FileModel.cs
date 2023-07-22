using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntrepreneurshipSchoolBackend.Migrations
{
    /// <inheritdoc />
    public partial class FileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Learner_LearnerId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Learner_TrackerId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attends_Learner_LearnerId",
                table: "Attends");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_ClaimStatuses_StatusId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_ClaimTypes_TypeId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Learner_LearnerId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Learner_ReceiverId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Lot_LotId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Tasks_TaskId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Lot_Learner_LearnerId",
                table: "Lot");

            migrationBuilder.DropForeignKey(
                name: "FK_Relates_Learner_LearnerId",
                table: "Relates");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Learner_LearnerId",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Claim_ClaimId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Learner_LearnerId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lot",
                table: "Lot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Learner",
                table: "Learner");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claim",
                table: "Claim");

            migrationBuilder.RenameTable(
                name: "Lot",
                newName: "Lots");

            migrationBuilder.RenameTable(
                name: "Learner",
                newName: "Learners");

            migrationBuilder.RenameTable(
                name: "Claim",
                newName: "Claims");

            migrationBuilder.RenameIndex(
                name: "IX_Lot_LearnerId",
                table: "Lots",
                newName: "IX_Lots_LearnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_TypeId",
                table: "Claims",
                newName: "IX_Claims_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_TaskId",
                table: "Claims",
                newName: "IX_Claims_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_StatusId",
                table: "Claims",
                newName: "IX_Claims_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_ReceiverId",
                table: "Claims",
                newName: "IX_Claims_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_LotId",
                table: "Claims",
                newName: "IX_Claims_LotId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_LearnerId",
                table: "Claims",
                newName: "IX_Claims_LearnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lots",
                table: "Lots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Learners",
                table: "Learners",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claims",
                table: "Claims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Learners_LearnerId",
                table: "Assessments",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Learners_TrackerId",
                table: "Assessments",
                column: "TrackerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attends_Learners_LearnerId",
                table: "Attends",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ClaimStatuses_StatusId",
                table: "Claims",
                column: "StatusId",
                principalTable: "ClaimStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ClaimTypes_TypeId",
                table: "Claims",
                column: "TypeId",
                principalTable: "ClaimTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Learners_LearnerId",
                table: "Claims",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Learners_ReceiverId",
                table: "Claims",
                column: "ReceiverId",
                principalTable: "Learners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Lots_LotId",
                table: "Claims",
                column: "LotId",
                principalTable: "Lots",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Tasks_TaskId",
                table: "Claims",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lots_Learners_LearnerId",
                table: "Lots",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relates_Learners_LearnerId",
                table: "Relates",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Learners_LearnerId",
                table: "Solutions",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Claims_ClaimId",
                table: "Transactions",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Learners_LearnerId",
                table: "Transactions",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Learners_LearnerId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Learners_TrackerId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attends_Learners_LearnerId",
                table: "Attends");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ClaimStatuses_StatusId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ClaimTypes_TypeId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Learners_LearnerId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Learners_ReceiverId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Lots_LotId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Tasks_TaskId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Lots_Learners_LearnerId",
                table: "Lots");

            migrationBuilder.DropForeignKey(
                name: "FK_Relates_Learners_LearnerId",
                table: "Relates");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Learners_LearnerId",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Claims_ClaimId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Learners_LearnerId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lots",
                table: "Lots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Learners",
                table: "Learners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claims",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "Lots",
                newName: "Lot");

            migrationBuilder.RenameTable(
                name: "Learners",
                newName: "Learner");

            migrationBuilder.RenameTable(
                name: "Claims",
                newName: "Claim");

            migrationBuilder.RenameIndex(
                name: "IX_Lots_LearnerId",
                table: "Lot",
                newName: "IX_Lot_LearnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_TypeId",
                table: "Claim",
                newName: "IX_Claim_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_TaskId",
                table: "Claim",
                newName: "IX_Claim_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_StatusId",
                table: "Claim",
                newName: "IX_Claim_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_ReceiverId",
                table: "Claim",
                newName: "IX_Claim_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_LotId",
                table: "Claim",
                newName: "IX_Claim_LotId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_LearnerId",
                table: "Claim",
                newName: "IX_Claim_LearnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lot",
                table: "Lot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Learner",
                table: "Learner",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claim",
                table: "Claim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Learner_LearnerId",
                table: "Assessments",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Learner_TrackerId",
                table: "Assessments",
                column: "TrackerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attends_Learner_LearnerId",
                table: "Attends",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_ClaimStatuses_StatusId",
                table: "Claim",
                column: "StatusId",
                principalTable: "ClaimStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_ClaimTypes_TypeId",
                table: "Claim",
                column: "TypeId",
                principalTable: "ClaimTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Learner_LearnerId",
                table: "Claim",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Learner_ReceiverId",
                table: "Claim",
                column: "ReceiverId",
                principalTable: "Learner",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Lot_LotId",
                table: "Claim",
                column: "LotId",
                principalTable: "Lot",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Tasks_TaskId",
                table: "Claim",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lot_Learner_LearnerId",
                table: "Lot",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relates_Learner_LearnerId",
                table: "Relates",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Learner_LearnerId",
                table: "Solutions",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Claim_ClaimId",
                table: "Transactions",
                column: "ClaimId",
                principalTable: "Claim",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Learner_LearnerId",
                table: "Transactions",
                column: "LearnerId",
                principalTable: "Learner",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

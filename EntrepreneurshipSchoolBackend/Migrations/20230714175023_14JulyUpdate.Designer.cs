﻿// <auto-generated />
using System;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EntrepreneurshipSchoolBackend.Migrations
{
    [DbContext(typeof(ApiDbContext))]
    [Migration("20230714175023_14JulyUpdate")]
    partial class _14JulyUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Assessments", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AssessmentsType")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Grade")
                        .HasColumnType("integer");

                    b.Property<int>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("TaskId")
                        .HasColumnType("integer");

                    b.Property<int>("TrackerId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LearnerId");

                    b.HasIndex("TaskId");

                    b.HasIndex("TrackerId");

                    b.ToTable("Assessments");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.AssessmentsType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AssessmentsTypes");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Attend", b =>
                {
                    b.Property<int>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("LessonId")
                        .HasColumnType("integer");

                    b.Property<char>("DidCome")
                        .HasColumnType("character(1)");

                    b.Property<int?>("TransactionId")
                        .HasColumnType("integer");

                    b.HasKey("LearnerId", "LessonId");

                    b.HasIndex("LessonId");

                    b.HasIndex("TransactionId");

                    b.ToTable("Attends");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Claim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<int?>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int?>("LotId")
                        .HasColumnType("integer");

                    b.Property<int?>("ReceiverId")
                        .HasColumnType("integer");

                    b.Property<int>("StatusId")
                        .HasColumnType("integer");

                    b.Property<int?>("Sum")
                        .HasColumnType("integer");

                    b.Property<int?>("TaskId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("TypeId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LearnerId");

                    b.HasIndex("LotId");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("StatusId");

                    b.HasIndex("TaskId");

                    b.HasIndex("TypeId");

                    b.ToTable("Claim");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.ClaimStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ClaimStatuses");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.ClaimType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ClaimTypes");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<string>("Theme")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Learner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Balance")
                        .HasColumnType("integer");

                    b.Property<string>("EmailLogin")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<char?>("Gender")
                        .HasColumnType("character(1)");

                    b.Property<double?>("GradeBonus")
                        .HasColumnType("double precision");

                    b.Property<char>("IsTracker")
                        .HasColumnType("character(1)");

                    b.Property<string>("Lastname")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Messenger")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("character varying(12)");

                    b.Property<double>("ResultGrade")
                        .HasColumnType("double precision");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.ToTable("Learner");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Lesson", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<string>("PresLink")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("VideoLink")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("Lessons");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Lot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<int?>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<string>("Performer")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("Price")
                        .HasColumnType("integer");

                    b.Property<string>("Terms")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("LearnerId");

                    b.ToTable("Lot");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Relate", b =>
                {
                    b.Property<int>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.HasKey("LearnerId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("Relates");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Solution", b =>
                {
                    b.Property<int>("TaskId")
                        .HasColumnType("integer");

                    b.Property<int>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CompleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("File")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TaskId", "LearnerId", "GroupId");

                    b.HasIndex("GroupId");

                    b.HasIndex("LearnerId");

                    b.ToTable("Solutions");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<string>("Criteria")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<DateTime>("Deadline")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("IsGroup")
                        .HasColumnType("boolean");

                    b.Property<int?>("LessonId")
                        .HasColumnType("integer");

                    b.Property<string>("Link")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("TypeId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LessonId");

                    b.HasIndex("TypeId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.TaskType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TaskTypes");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("ClaimId")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LearnerId")
                        .HasColumnType("integer");

                    b.Property<int>("Sum")
                        .HasColumnType("integer");

                    b.Property<int>("TypeId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClaimId");

                    b.HasIndex("LearnerId");

                    b.HasIndex("TypeId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.TransactionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TransactionTypes");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Assessments", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Tracker")
                        .WithMany()
                        .HasForeignKey("TrackerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Learner");

                    b.Navigation("Task");

                    b.Navigation("Tracker");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Attend", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Lesson", "Lesson")
                        .WithMany()
                        .HasForeignKey("LessonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Transaction", "Transaction")
                        .WithMany()
                        .HasForeignKey("TransactionId");

                    b.Navigation("Learner");

                    b.Navigation("Lesson");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Claim", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Lot", "Lot")
                        .WithMany()
                        .HasForeignKey("LotId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Receiver")
                        .WithMany()
                        .HasForeignKey("ReceiverId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.ClaimStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.ClaimType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Learner");

                    b.Navigation("Lot");

                    b.Navigation("Receiver");

                    b.Navigation("Status");

                    b.Navigation("Task");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Lot", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId");

                    b.Navigation("Learner");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Relate", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Learner");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Solution", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Learner");

                    b.Navigation("Task");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Task", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Lesson", "Lesson")
                        .WithMany()
                        .HasForeignKey("LessonId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.TaskType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Lesson");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EntrepreneurshipSchoolBackend.Models.Transaction", b =>
                {
                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Claim", "Claim")
                        .WithMany()
                        .HasForeignKey("ClaimId");

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.Learner", "Learner")
                        .WithMany()
                        .HasForeignKey("LearnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EntrepreneurshipSchoolBackend.Models.TransactionType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Claim");

                    b.Navigation("Learner");

                    b.Navigation("Type");
                });
#pragma warning restore 612, 618
        }
    }
}

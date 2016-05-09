﻿using Microsoft.AspNet.Identity;
using MooshakPP.Models;
using MooshakPP.Models.Entities;
using MooshakPP.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MooshakPP.Services
{
    public class TeacherService : StudentService
    {
        private ApplicationDbContext db;

        public TeacherService()
        {
            db = new ApplicationDbContext();
        }

        public CreateAssignmentViewModel AddAssignment(string userID, int courseID, int assignmentID)
        {
            CreateAssignmentViewModel allAssignments = new CreateAssignmentViewModel();
            allAssignments.courses = GetCourses(userID);
            allAssignments.assignments = new List<Assignment>(GetAssignments(courseID));
            allAssignments.currentCourse = GetCourseByID(courseID);
            allAssignments.currentAssignment = GetAssignmentByID(assignmentID);
            

            //created a single assignment for the Post request in the Teacher controller
            //so now we have a courseID for our new assignment
            //allAssignments.newAssignment = new Assignment();
            //allAssignments.newAssignment.courseID = courseID;

            return allAssignments;
        }
        
        public bool CreateAssignment(Assignment newAssignment)
        {
            try
            {
                db.Assignments.Add(newAssignment);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public CreateMilestoneViewModel AddMilestone(int assId, int? currMilestoneId)
        {
            CreateMilestoneViewModel model = new CreateMilestoneViewModel();
            model.milestones = GetMilestones(assId);
            if(currMilestoneId == null)
            {
                model.currentMilestone = new Milestone();
                model.currentMilestone.assignmentID = 8; // assId;
            }
            else
            {
                model.currentMilestone = (from Milestone m in model.milestones
                                          where m.ID == currMilestoneId
                                          select m).FirstOrDefault();
            }
            model.currentAssignment = GetAssignmentByID(assId);
            
            return model;
        }

        public bool CreateMilestones(List<Milestone> milestones)
        {
            return true;
        }

        public bool RemoveAssignment(int assignmentID)
        {
            Assignment assignment = GetAssignmentByID(assignmentID);
            if (assignment != null)
            {
                db.Assignments.Remove(assignment);
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
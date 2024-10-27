using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace StudentManagementApp.Controllers
{
    public class ClassController : Controller
    {
        private string GetXmlFilePath() =>
            Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Classes.xml");

        // Hiển thị danh sách lớp
        public IActionResult Index()
        {
            var classes = GetClasses();
            ViewBag.Classes = classes;
            return View();
        }

        // Lấy danh sách lớp từ XML
        private List<string> GetClasses()
        {
            var xmlDoc = XDocument.Load(GetXmlFilePath());
            return xmlDoc.Descendants("Class")
                         .Select(c => c.Attribute("Name")?.Value)
                         .ToList();
        }

        // Thêm lớp mới
        [HttpPost]
        public IActionResult AddClass(string className)
        {
            var xmlDoc = XDocument.Load(GetXmlFilePath());
            if (xmlDoc.Descendants("Class").Any(c => c.Attribute("Name")?.Value == className))
            {
                ViewBag.Message = "Lớp đã tồn tại!";
            }
            else
            {
                xmlDoc.Root.Add(new XElement("Class", new XAttribute("Name", className)));
                xmlDoc.Save(GetXmlFilePath());
                ViewBag.Message = "Thêm lớp thành công!";
            }
            return RedirectToAction("Index");
        }

        // Xoá lớp
        public IActionResult DeleteClass(string className)
        {
            var xmlDoc = XDocument.Load(GetXmlFilePath());
            var classElement = xmlDoc.Descendants("Class")
                                     .FirstOrDefault(c => c.Attribute("Name")?.Value == className);

            if (classElement != null)
            {
                classElement.Remove();
                xmlDoc.Save(GetXmlFilePath());
                ViewBag.Message = "Xoá lớp thành công!";
            }
            else
            {
                ViewBag.Message = "Lớp không tồn tại!";
            }

            return RedirectToAction("Index");
        }

        // Chuyển hướng đến thông tin sinh viên
        public IActionResult Students(string className)
        {
            ViewBag.ClassName = className;
            var students = GetStudents(className);
            return View(students);
        }
        
        //THêm sinh viên
        [HttpPost]
        public IActionResult AddStudent(string className, string id, string fullName, int birthYear, string gender, string phone, string address)
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Students.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);

            // Check if student with the same ID already exists
            if (xmlDoc.Descendants("Student").Any(s => s.Element("Id")?.Value == id))
            {
                ViewBag.Message = "Mã sinh viên đã tồn tại!";
            }
            else
            {
                // Add new student
                xmlDoc.Root.Add(new XElement("Student",
                    new XElement("Id", id),
                    new XElement("FullName", fullName),
                    new XElement("BirthYear", birthYear),
                    new XElement("Gender", gender),
                    new XElement("Phone", phone),
                    new XElement("Address", address),
                    new XElement("ClassName", className)
                ));
                xmlDoc.Save(xmlFilePath);
                ViewBag.Message = "Thêm sinh viên thành công!";
            }

            return RedirectToAction("Students", new { className });
        }
        
        // Chuyển hướng đến trang chỉnh sửa sinh viên
        // public IActionResult Edit(string id, string className)
        // {
        //     ViewBag.ClassName = className;
        //     var student = GetStudentById(id); // Retrieve the student by ID
        //     return View(student);
        // }

        // Lấy thông tin sinh viên theo ID
        private Student GetStudentById(string id)
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Students.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);
            
            var studentElement = xmlDoc.Descendants("Student").FirstOrDefault(s => s.Element("Id")?.Value == id);
            
            if (studentElement != null)
            {
                return new Student
                {
                    Id = studentElement.Element("Id")?.Value,
                    FullName = studentElement.Element("FullName")?.Value,
                    BirthYear = int.TryParse(studentElement.Element("BirthYear")?.Value, out int birthYear) ? birthYear : 0,
                    Gender = studentElement.Element("Gender")?.Value,
                    Phone = studentElement.Element("Phone")?.Value,
                    Address = studentElement.Element("Address")?.Value
                };
            }
            return null;
        }

        // Xử lý chỉnh sửa sinh viên
        [HttpPost]
        public IActionResult EditStudent(string id, string className, string fullName, int birthYear, string gender, string phone, string address)
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Students.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);
            
            var studentElement = xmlDoc.Descendants("Student").FirstOrDefault(s => s.Element("Id")?.Value == id);
            
            if (studentElement != null)
            {
                // Update student details
                studentElement.SetElementValue("FullName", fullName);
                studentElement.SetElementValue("BirthYear", birthYear);
                studentElement.SetElementValue("Gender", gender);
                studentElement.SetElementValue("Phone", phone);
                studentElement.SetElementValue("Address", address);
                xmlDoc.Save(xmlFilePath);
                ViewBag.Message = "Chỉnh sửa sinh viên thành công!";
            }
            else
            {
                ViewBag.Message = "Sinh viên không tồn tại!";
            }

            return RedirectToAction("Students", new { className });
        }

        
        // Xoá sinh viên
        [HttpPost]
        public IActionResult DeleteStudent(string id, string className)
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Students.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);
            var studentElement = xmlDoc.Descendants("Student").FirstOrDefault(s => s.Element("Id")?.Value == id);

            if (studentElement != null)
            {
                studentElement.Remove();
                xmlDoc.Save(xmlFilePath);
                ViewBag.Message = "Xoá sinh viên thành công!";
            }
            else
            {
                ViewBag.Message = "Sinh viên không tồn tại!";
            }
            return RedirectToAction("Students", new { className });
        }


        // Lấy danh sách sinh viên từ XML
        private List<Student> GetStudents(string className)
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Students.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);

            return xmlDoc.Descendants("Student")
                        .Where(s => s.Element("ClassName")?.Value == className) // Lọc sinh viên theo lớp
                        .Select(s => new Student
                        {
                            Id = s.Element("Id")?.Value,
                            FullName = s.Element("FullName")?.Value,
                            BirthYear = int.TryParse(s.Element("BirthYear")?.Value, out int birthYear) ? birthYear : 0,
                            Gender = s.Element("Gender")?.Value,
                            Phone = s.Element("Phone")?.Value,
                            Address = s.Element("Address")?.Value
                        })
                        .ToList();
        }
    }   

    public class Student
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public int BirthYear { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}

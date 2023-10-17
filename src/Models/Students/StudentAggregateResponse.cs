using src.Models.Assessments;
using src.Models.Courses;
using src.Models.Grades;
using src.Models.ProgramAssociations;
using src.Models.SectionAssociations;

namespace src.Models.Students;
public class StudentAggregateResponse
{   
    public IEnumerable<AssessmentResponse> Assessments { get; set;} = Enumerable.Empty<AssessmentResponse>();
    public IEnumerable<ProgramAssociationResponse> ProgramAssociations { get; set;} = Enumerable.Empty<ProgramAssociationResponse>();
    public IEnumerable<SectionAssociationResponse> SectionAssociations { get; set;}= Enumerable.Empty<SectionAssociationResponse>();
    public IEnumerable<CourseTranscriptResponse> CourseTranscripts { get; set;} = Enumerable.Empty<CourseTranscriptResponse>();
    public IEnumerable<GradeResponse> Grades { get; set;} = Enumerable.Empty<GradeResponse>();
    
}

//assesments..
// 'http://localhost:8022/api/data/v3/ed-fi/studentAssessments?offset=0&limit=25&totalCount=false&studentUniqueId=605770'
//program associations
// 'http://localhost:8022/api/data/v3/ed-fi/studentProgramAssociations?offset=0&limit=25&totalCount=false&studentUniqueId=604854'
//Section associations
// http://localhost:8022/api/data/v3/ed-fi/studentSectionAssociations?offset=0&limit=25&totalCount=false&studentUniqueId=605770
//Courses Transcripts..
// http://localhost:8022/api/data/v3/ed-fi/courseTranscripts?offset=0&limit=25&totalCount=false&studentUniqueId=605770
//Grades
// http://localhost:8022/api/data/v3/ed-fi/grades?offset=0&limit=25&totalCount=false&studentUniqueId=605770

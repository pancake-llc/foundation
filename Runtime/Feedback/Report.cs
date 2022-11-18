using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pancake.Feedback
{
    public class Report
    {
        // Trello limit is 100 attachments per card, screenshot counts as one
        private const int MAX_ATTACHMENTS = 99;

        /// <summary>
        /// Labels to add to the card on Trello
        /// </summary>
        public readonly List<Label> Labels = new List<Label>();

        /// <summary>
        /// Report information stored by title
        /// </summary>
        private readonly Dictionary<string, ReportSection> info;

        /// <summary>
        /// Trello list this report will be added to
        /// </summary>
        public List List;

        /// <summary>
        /// Binary data for screenshot to be included with this report
        /// </summary>
        public byte[] Screenshot;

        /// <summary>
        /// The title of the card on Trello
        /// </summary>
        public string Title;

        /// <summary>
        /// Additional files attached to this report
        /// </summary>
        /// <remarks>
        /// Private to enforce Trello attachment limit (100)
        /// </remarks>
        public List<FileAttachment> Attachments { get; }

        /// <summary>
        /// Returns a section in the report by title
        /// </summary>
        /// <param name="sectionTitle"></param>
        /// <returns></returns>
        public ReportSection this[string sectionTitle]
        {
            get
            {
                if (info.ContainsKey(sectionTitle)) return info[sectionTitle];

                Debug.LogError("Report does not contain a section with title \"" + sectionTitle + "\"");
                return null;
            }
            set
            {
                if (info.ContainsKey(sectionTitle))
                    info[sectionTitle] = value;
                else
                    Debug.LogError("Report does not contain a section with title \"" + sectionTitle + "\"");
            }
        }


        public Report()
        {
            // initialize info collection
            info = new Dictionary<string, ReportSection>();

            // initialize attachments list
            Attachments = new List<FileAttachment>();
        }

        /// <summary>
        /// Adds a new empty section to the report
        /// </summary>
        /// <param name="title">The title of the section</param>
        /// <param name="sortOrder">The order of the section on the report (lowest first)</param>
        public void AddSection(string title, int sortOrder = 0) { AddSection(new ReportSection(title, sortOrder)); }

        /// <summary>
        /// Adds a new section to the report
        /// </summary>
        /// <param name="section"></param>
        public void AddSection(ReportSection section)
        {
            if (info.ContainsKey(section.Title))
            {
                // Do we want to eventually support multiple sections sharing the same title?
                Debug.LogError("Report already contains a section with title \"" + section.Title + "\"");
                return;
            }

            // add the section to the dictionary
            info.Add(section.Title, section);
        }

        public void RemoveSection(string title)
        {
            if (!info.ContainsKey(title))
            {
                Debug.LogWarning("Can not remove section \"" + title + "\" because report does not contain a section with that name");
                return;
            }

            info.Remove(title);
        }

        /// <summary>
        /// Checks whether the report already has a section
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool HasSection(string title) { return info.ContainsKey(title); }

        /// <summary>
        /// Returns the report formatted in markdown for Trello
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var report = new StringBuilder();

            // sort report sections
            var sections = info.Select(r => r.Value).OrderBy(v => v.SortOrder).ToArray();

            // build report string
            foreach (var t in sections)
                report.AppendLine(t.ToString());

            return report.ToString();
        }

        public string GetLocalFileText()
        {
            var report = new StringBuilder();

            // add category and label
            report.AppendLine(Markdown.H3("Category"));
            report.AppendLine();
            report.AppendLine(List.name);
            report.AppendLine();

            report.AppendLine(Markdown.H3("Labels"));
            report.AppendLine();
            foreach (var label in Labels) report.AppendLine("- " + label.name);
            report.AppendLine();

            // add the rest of the report
            report.AppendLine(ToString());

            return report.ToString();
        }

        /// <summary>
        /// Attach a file to the report
        /// </summary>
        /// <param name="file"></param>
        public void AttachFile(FileAttachment file)
        {
            if (Attachments.Count + 1 > MAX_ATTACHMENTS)
            {
                Debug.LogError("Error attaching file: maximum attachment limit (" + MAX_ATTACHMENTS + ") reached!");
                return;
            }

            Attachments.Add(file);
        }

        /// <summary>
        /// Attach a file to the report
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="filePath">The path to the file</param>
        public void AttachFile(string name, string filePath) { AttachFile(new FileAttachment(name, filePath, null)); }

        /// <summary>
        /// Attach a file to the report
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="data">The file data</param>
        public void AttachFile(string name, byte[] data) { AttachFile(new FileAttachment(name, data)); }

        /// <summary>
        /// Adds a label to the report.
        /// </summary>
        public void AddLabel(Label label)
        {
            if (HasLabel(label))
            {
                Debug.LogWarning("The report already has the label \"" + label.name + "\"");
                return;
            }

            Labels.Add(label);
        }

        /// <summary>
        /// Checks if the report already has a label.
        /// </summary>
        public bool HasLabel(Label label) => Labels.Contains(label);
    }
}
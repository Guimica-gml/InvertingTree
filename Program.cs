using System.Diagnostics;

namespace CSharpSomething {
    public class Program {
        public static void Main(string[] args) {
            // Comment lines with 'GenerateGraph' if you don't have graphviz installed

            var init_value = 1;
            var root = GenerateTree(ref init_value, 3);
            root?.Print();
            root?.GenerateGraph(@".\graphs", "root");

            Console.WriteLine("-------------");

            var inverted = root?.Invert();
            inverted?.Print();
            inverted?.GenerateGraph(@".\graphs", "inverted");
        }

        // This function is irrelevant to the TreeNode implementation
        // So that's why it's outside TreeNode's class
        // That's just meant to be a fast way to create a tree for testing
        public static TreeNode<int>? GenerateTree(ref int init_value, int depth_level) {
            if (depth_level <= 0) {
                return null;
            }

            init_value += 1;
            return new TreeNode<int>(
                init_value - 1,
                GenerateTree(ref init_value, depth_level - 1),
                GenerateTree(ref init_value, depth_level - 1)
            );
        }
    }

    public class TreeNode<T> {
        public T value;
        public TreeNode<T>? left;
        public TreeNode<T>? right;

        public TreeNode(T value, TreeNode<T>? left, TreeNode<T>? right ) {
            this.value = value;
            this.left = left;
            this.right = right;
        }

        // Uses graphviz to create a visual representation of the TreeNode
        public void GenerateGraph(string folderpath, string filename) {
            var dotfilepath = Path.Join(folderpath, $"{filename}.dot");
            var svgfilepath = Path.Join(folderpath, $"{filename}.svg");

            File.AppendAllText(dotfilepath, $"graph {filename} {{\n");
            File.AppendAllText(dotfilepath, "    graph [ordering=\"out\"];\n");
            WriteToFile(dotfilepath);
            File.AppendAllText(dotfilepath, "}\n");

            using (var dot = new Process()) {
                dot.StartInfo.Verb = "runas";
                dot.StartInfo.FileName = "dot";
                dot.StartInfo.Arguments = $"-Tsvg {dotfilepath} -o {svgfilepath}";
                dot.StartInfo.CreateNoWindow = true;
                dot.Start();
                dot.WaitForExit();
            }

            File.Delete(dotfilepath);
        }

        private void WriteToFile(string filepath) {
            if (left != null) {
                File.AppendAllText(filepath, $"    {value} -- {left.value}\n");
                left.WriteToFile(filepath);
            }
            if (right != null) {
                File.AppendAllText(filepath, $"    {value} -- {right.value}\n");
                right.WriteToFile(filepath);
            }
        }

        // Depending on the value inside the TreeNode
        // the text printed might not be displayed in a good way
        // Usually if there are big numbers or if you use another data type
        public void Print(int level = 0) {
            right?.Print(level + 1);
            for (var i = 0; i < level; ++i) {
                Console.Write("   ");
            }
            Console.WriteLine(value?.ToString());
            left?.Print(level + 1);
        }

        // Note: if T is an object, it won't clone it
        // It uses shallow copy, so it will be a reference to the same object
        public TreeNode<T> Invert() {
            var copy = (TreeNode<T>)this.MemberwiseClone();
            var save_right = copy.right;

            copy.right = copy.left?.Invert();
            copy.left = save_right?.Invert();

            return copy;
        }
    }
}

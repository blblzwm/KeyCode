// ST005: Conditional questions are provisional because the supplied
// 2.2 lesson contains tuple material.
// ST007: Questions follow the Lists lesson, not the function practice item.

(function (root, factory) {
    const questions = factory();

    if (typeof module === "object" && module.exports) {
        module.exports = questions;
    } else {
        root.CodeQuestQuestions = questions;
    }
}(typeof globalThis !== "undefined" ? globalThis : this, function () {
    return [
        // ===== ST001: Introduction to Python =====
        {
            id: "RQ001",
            subtopicID: "ST001",
            topic: "1.1 Introduction to Python",
            tier: 1,
            code: '# print("Stop")\nprint("Run!")',
            options: ["Stop", "Run!", "Stop\nRun!", "# Run!"],
            answer: 1,
            explanation: "The commented line is ignored. Only the active print statement displays a message."
        },
        {
            id: "RQ002",
            subtopicID: "ST001",
            topic: "1.1 Introduction to Python",
            tier: 1,
            code: 'print("Ready")\nprint("Jump")',
            options: ["Ready Jump", "Jump\nReady", "Ready\nJump", "ReadyJump"],
            answer: 2,
            explanation: "Each print statement starts a new output line."
        },
        {
            id: "RQ003",
            subtopicID: "ST001",
            topic: "1.1 Introduction to Python",
            tier: 1,
            code: 'print("# checkpoint")',
            options: ["checkpoint", "# checkpoint", "No output", "Error"],
            answer: 1,
            explanation: "The # is inside quotation marks, so it is text, not a comment."
        },

        // ===== ST002: Variables and Data Types =====
        {
            id: "RQ004",
            subtopicID: "ST002",
            topic: "1.2 Variables and Data Types",
            tier: 1,
            code: 'lane = "left"\nlane = "right"\nprint(lane)',
            options: ["left", "right", "left right", "lane"],
            answer: 1,
            explanation: "The second assignment replaces the value stored in lane."
        },
        {
            id: "RQ005",
            subtopicID: "ST002",
            topic: "1.2 Variables and Data Types",
            tier: 1,
            code: 'speed = "8"\nprint(type(speed))',
            options: [
                "<class 'int'>",
                "<class 'float'>",
                "<class 'str'>",
                "<class 'bool'>"
            ],
            answer: 2,
            explanation: "Quotation marks make 8 text, even though it looks like a number."
        },
        {
            id: "RQ006",
            subtopicID: "ST002",
            topic: "1.2 Variables and Data Types",
            tier: 1,
            code: 'boost = "12"\nprint(int(boost))',
            options: ['"12"', "12.0", "True", "12"],
            answer: 3,
            explanation: "int converts the numeric text to an integer. print displays 12 without quotation marks."
        },

        // ===== ST003: Input and Output =====
        {
            id: "RQ007",
            subtopicID: "ST003",
            topic: "1.3 Input and Output",
            tier: 1,
            code: '# The player types Kai, then presses Enter.\nname = input()\nprint("Go", name)',
            inputs: ["Kai"],
            options: ["GoKai", "Go name", "Go Kai", "Kai Go"],
            answer: 2,
            explanation: "input returns Kai. Printing two values with a comma adds a space."
        },
        {
            id: "RQ008",
            subtopicID: "ST003",
            topic: "1.3 Input and Output",
            tier: 1,
            code: "# The player types 7, then presses Enter.\ncoins = int(input())\nprint(coins + 2)",
            inputs: ["7"],
            options: ["72", "9", "7", "Error"],
            answer: 1,
            explanation: "The input is converted to an integer before 2 is added."
        },
        {
            id: "RQ009",
            subtopicID: "ST003",
            topic: "1.3 Input and Output",
            tier: 1,
            code: 'print("Run", "Jump", sep="-", end="!")',
            options: ["Run Jump!", "Run-Jump!", "Run-Jump", "Run!Jump!"],
            answer: 1,
            explanation: "sep places a hyphen between the words; end places an exclamation mark at the end."
        },

        // ===== ST004: Comparison and Logical Controls =====
        {
            id: "RQ010",
            subtopicID: "ST004",
            topic: "2.1 Comparison and Logical Controls",
            tier: 2,
            code: "coins = 8\nprint(coins >= 8)",
            options: ["True", "False", "8", "0"],
            answer: 0,
            explanation: "Greater than or equal to includes the boundary value 8."
        },
        {
            id: "RQ011",
            subtopicID: "ST004",
            topic: "2.1 Comparison and Logical Controls",
            tier: 2,
            code: "has_key = True\ngate_open = False\nprint(has_key and gate_open)",
            options: ["True", "False", "None", "Error"],
            answer: 1,
            explanation: "and needs both conditions to be true. The gate is not open."
        },
        {
            id: "RQ012",
            subtopicID: "ST004",
            topic: "2.1 Comparison and Logical Controls",
            tier: 2,
            code: "shield = False\nprint(not shield)",
            options: ["False", "shield", "True", "None"],
            answer: 2,
            explanation: "not reverses False to True."
        },

        // ===== ST005: Conditional Statements =====
        {
            id: "RQ013",
            subtopicID: "ST005",
            topic: "2.2 Conditional Statements",
            tier: 2,
            code: 'coins = 3\nif coins >= 5:\n    print("Boost")\nelse:\n    print("Run")',
            options: ["Boost", "Run", "Boost\nRun", "No output"],
            answer: 1,
            explanation: "3 does not meet the condition, so the else branch displays Run."
        },
        {
            id: "RQ014",
            subtopicID: "ST005",
            topic: "2.2 Conditional Statements",
            tier: 2,
            code: 'score = 20\nif score >= 30:\n    print("Gold")\nelif score >= 10:\n    print("Silver")\nelse:\n    print("Bronze")',
            options: ["Gold", "Silver", "Bronze", "Gold\nSilver"],
            answer: 1,
            explanation: "The first condition is false, but 20 is at least 10, so the elif branch runs."
        },
        {
            id: "RQ015",
            subtopicID: "ST005",
            topic: "2.2 Conditional Statements",
            tier: 2,
            code: 'shield = True\nif shield:\n    print("Safe")\nprint("Go")',
            options: ["Safe", "Go", "Safe\nGo", "No output"],
            answer: 2,
            explanation: "The condition is true. The final print is outside the block and also runs."
        },

        // ===== ST006: Loops =====
        {
            id: "RQ016",
            subtopicID: "ST006",
            topic: "2.3 Loops",
            tier: 2,
            code: "for gate in range(1, 4):\n    print(gate)",
            options: [
                "0\n1\n2\n3",
                "1\n2\n3\n4",
                "1\n2\n3",
                "1\n4"
            ],
            answer: 2,
            explanation: "range includes the starting value 1 and stops before 4."
        },
        {
            id: "RQ017",
            subtopicID: "ST006",
            topic: "2.3 Loops",
            tier: 2,
            code: "for step in range(4):\n    if step == 2:\n        break\n    print(step)",
            options: ["0\n1", "0\n1\n2", "0\n1\n3", "2"],
            answer: 0,
            explanation: "At step 2, break ends the loop before the print statement."
        },
        {
            id: "RQ018",
            subtopicID: "ST006",
            topic: "2.3 Loops",
            tier: 2,
            code: "for lane in range(3):\n    if lane == 1:\n        continue\n    print(lane)",
            options: ["0", "0\n1\n2", "1\n2", "0\n2"],
            answer: 3,
            explanation: "continue skips the print for lane 1, then the loop proceeds to lane 2."
        },

        // ===== ST007: Lists =====
        {
            id: "RQ019",
            subtopicID: "ST007",
            topic: "3.1 Lists",
            tier: 3,
            code: 'bag = ["coin", "shield", "boot"]\nprint(bag[1])',
            options: ["coin", "shield", "boot", "1"],
            answer: 1,
            explanation: "List indexes start at zero, so index 1 selects shield."
        },
        {
            id: "RQ020",
            subtopicID: "ST007",
            topic: "3.1 Lists",
            tier: 3,
            code: 'bag = ["coin"]\nbag.append("key")\nprint(bag)',
            options: [
                "['key']",
                "['coin', 'key']",
                "['key', 'coin']",
                "['coin']"
            ],
            answer: 1,
            explanation: "append adds key at the end of the existing list."
        },
        {
            id: "RQ021",
            subtopicID: "ST007",
            topic: "3.1 Lists",
            tier: 3,
            code: 'bag = ["coin", "rock", "key"]\nbag.remove("rock")\nprint(bag)',
            options: [
                "['coin', 'key']",
                "['rock']",
                "['coin', 'rock']",
                "[]"
            ],
            answer: 0,
            explanation: "remove deletes the matching item rock and keeps the other items in order."
        },

        // ===== ST008: Tuple =====
        {
            id: "RQ022",
            subtopicID: "ST008",
            topic: "3.2 Tuple",
            tier: 3,
            code: "checkpoint = (4, 9)\nprint(checkpoint[0])",
            options: ["9", "0", "4", "(4, 9)"],
            answer: 2,
            explanation: "Tuple indexing starts at zero. The first coordinate is 4."
        },
        {
            id: "RQ023",
            subtopicID: "ST008",
            topic: "3.2 Tuple",
            tier: 3,
            code: 'route = ("left", "up", "right")\nprint(route[2])',
            options: ["left", "up", "right", "Error"],
            answer: 2,
            explanation: "Index 2 selects the third tuple item, right."
        },
        {
            id: "RQ024",
            subtopicID: "ST008",
            topic: "3.2 Tuple",
            tier: 3,
            code: "spawn = (2, 6)\nprint(type(spawn))",
            options: [
                "<class 'list'>",
                "<class 'tuple'>",
                "<class 'set'>",
                "<class 'int'>"
            ],
            answer: 1,
            explanation: "Comma-separated values inside parentheses create a tuple, suitable for a fixed spawn point."
        },

        // ===== ST009: Set =====
        {
            id: "RQ025",
            subtopicID: "ST009",
            topic: "3.3 Set",
            tier: 3,
            code: 'badges = {"star", "star"}\nprint(badges)',
            options: [
                "{'star', 'star'}",
                "['star', 'star']",
                "{'star'}",
                "{}"
            ],
            answer: 2,
            explanation: "A set keeps only one copy of each value. There is only one distinct badge."
        },
        {
            id: "RQ026",
            subtopicID: "ST009",
            topic: "3.3 Set",
            tier: 3,
            code: 'badges = {"star"}\nbadges.add("star")\nprint(badges)',
            options: [
                "{'star'}",
                "{'star', 'star'}",
                "{}",
                "Error"
            ],
            answer: 0,
            explanation: "Adding an existing set value does not create a duplicate."
        },
        {
            id: "RQ027",
            subtopicID: "ST009",
            topic: "3.3 Set",
            tier: 3,
            code: 'badges = {"star", "moon"}\nbadges.remove("moon")\nprint(badges)',
            options: [
                "{'moon'}",
                "{'star', 'moon'}",
                "{}",
                "{'star'}"
            ],
            answer: 3,
            explanation: "Removing moon leaves only star. A one-item set avoids relying on set display order."
        }
    ];
}));